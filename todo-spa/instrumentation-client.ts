// Set up performance monitoring
performance.mark("app-init");

// Set up error tracking
window.addEventListener("error", (event) => {
  // Send to your error tracking service
  reportError(event.error);
});

import { ZipkinExporter } from "@opentelemetry/exporter-zipkin";
import { registerInstrumentations } from "@opentelemetry/instrumentation";
import { DocumentLoadInstrumentation } from "@opentelemetry/instrumentation-document-load";
import { FetchInstrumentation } from "@opentelemetry/instrumentation-fetch";
import { UserInteractionInstrumentation } from "@opentelemetry/instrumentation-user-interaction";
import { defaultResource } from "@opentelemetry/resources";
import { BatchSpanProcessor } from "@opentelemetry/sdk-trace-base";
import { WebTracerProvider } from "@opentelemetry/sdk-trace-web";
import { ATTR_SERVICE_NAME } from "@opentelemetry/semantic-conventions";

const zipkinExporter = new ZipkinExporter({
  url: "http://localhost:9411/api/v2/spans", // http://zipkin:9411 does not work on localhost
  serviceName: "todo-spa-browser",
});

const nameResource = defaultResource();

nameResource.attributes[ATTR_SERVICE_NAME] = "todo-spa-browser";

const provider = new WebTracerProvider({
  spanProcessors: [new BatchSpanProcessor(zipkinExporter)],
  resource: nameResource,
});

registerInstrumentations({
  instrumentations: [
    new DocumentLoadInstrumentation(),
    new UserInteractionInstrumentation(),
    new FetchInstrumentation({
      propagateTraceHeaderCorsUrls: /.*/,
    }),
  ],

  tracerProvider: provider,
});

console.log("OpenTelemetry client initialized");
