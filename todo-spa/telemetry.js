import { PrometheusExporter } from "@opentelemetry/exporter-prometheus";
import { HostMetrics } from "@opentelemetry/host-metrics";
import { HttpInstrumentation } from "@opentelemetry/instrumentation-http";

import { ZipkinExporter } from "@opentelemetry/exporter-zipkin";
import { registerInstrumentations } from "@opentelemetry/instrumentation";
import {
  MeterProvider,
  PeriodicExportingMetricReader,
} from "@opentelemetry/sdk-metrics";

import { NodeTracerProvider } from "@opentelemetry/sdk-trace-node";

import { BatchSpanProcessor } from "@opentelemetry/sdk-trace-base";

const prometheusPort = 9464;

const exporter = new PrometheusExporter(
  { port: prometheusPort, preventServerStart: false },
  (error) => {
    if (error) {
      console.error("Error while starting telemetry", error);
    } else {
      console.log(
        "Prometheus scrape endpoint: http://localhost:%s/metrics",
        prometheusPort
      );
    }
  }
);

const meterProvider = new MeterProvider();
meterProvider.addMetricReader(new PeriodicExportingMetricReader({ exporter }));

registerInstrumentations({
  instrumentations: [new HttpInstrumentation(), new HostMetrics()],
});

if (process.env.ZIPKIN_URL) {
  const tracerProvider = new NodeTracerProvider();

  const zipkinExporter = new ZipkinExporter({
    url: process.env.ZIPKIN_URL,
    serviceName: "todo-spa",
  });

  tracerProvider.addSpanProcessor(new BatchSpanProcessor(zipkinExporter));
  tracerProvider.register();

  console.log(`Zipkin tracing enabled: ${process.env.ZIPKIN_URL}`);
} else {
  console.log("Zipkin tracing not enabled (set ZIPKIN_URL to activate)");
}

console.log("Telemetry started");
