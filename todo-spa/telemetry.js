import { PrometheusExporter } from "@opentelemetry/exporter-prometheus";
import { HostMetrics } from "@opentelemetry/host-metrics";
import { HttpInstrumentation } from "@opentelemetry/instrumentation-http";

import { registerInstrumentations } from "@opentelemetry/instrumentation";
import {
  MeterProvider,
  PeriodicExportingMetricReader,
} from "@opentelemetry/sdk-metrics";

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

console.log("Telemetry started");
