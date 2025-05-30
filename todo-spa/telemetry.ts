import { PrometheusExporter } from "@opentelemetry/exporter-prometheus";
import { ZipkinExporter } from "@opentelemetry/exporter-zipkin";
import { HostMetrics } from "@opentelemetry/host-metrics";
import { registerInstrumentations } from "@opentelemetry/instrumentation";
import { HttpInstrumentation } from "@opentelemetry/instrumentation-http";
import { RuntimeNodeInstrumentation } from "@opentelemetry/instrumentation-runtime-node";
import {
  defaultResource,
  detectResources,
  envDetector,
  hostDetector,
  processDetector,
} from "@opentelemetry/resources";
import { MeterProvider } from "@opentelemetry/sdk-metrics";
import { BatchSpanProcessor } from "@opentelemetry/sdk-trace-base";
import { NodeTracerProvider } from "@opentelemetry/sdk-trace-node";
import { ATTR_SERVICE_NAME } from "@opentelemetry/semantic-conventions";

const prometheusPort = 9464;

const exporter = new PrometheusExporter({
  port: prometheusPort,
  endpoint: "/metrics",
  host: "0.0.0.0", // Listen on all network interfaces
});

const detectedResources = detectResources({
  detectors: [envDetector, processDetector, hostDetector],
});

const nameResource = defaultResource();

nameResource.attributes[ATTR_SERVICE_NAME] = "todo-spa";

const meterProvider = new MeterProvider({
  readers: [exporter],
  resource: detectedResources.merge(nameResource),
});

const hostMetrics = new HostMetrics({
  name: `todo-spa`,
  meterProvider,
});

if (!process.env.ZIPKIN_URL) {
  throw new Error("ZIPKIN_URL not defined)");
}

const zipkinExporter = new ZipkinExporter({
  url: process.env.ZIPKIN_URL,
  serviceName: "todo-spa",
});

const tracerProvider = new NodeTracerProvider({
  spanProcessors: [new BatchSpanProcessor(zipkinExporter)],
  resource: nameResource,
});

registerInstrumentations({
  meterProvider,
  instrumentations: [
    new HttpInstrumentation(),
    new RuntimeNodeInstrumentation(),
  ],
  tracerProvider,
});

hostMetrics.start();

console.log("Telemetry started");
