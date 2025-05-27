import express from "express";
import { fetchFromDatabase, kafkaToDatabase } from "./link.js";
import promMid from "express-prometheus-middleware";
const {
  Tracer,
  ExplicitContext,
  BatchRecorder,
  jsonEncoder,
} = require("zipkin");
const { HttpLogger } = require("zipkin-transport-http");
const zipkinMiddleware =
  require("zipkin-instrumentation-express").expressMiddleware;

const ZIPKIN_ENDPOINT = process.env.ZIPKIN_ENDPOINT;

if (!ZIPKIN_ENDPOINT) {
  throw new Error("ZIPKIN_ENDPOINT not defined");
}

const tracer = new Tracer({
  ctxImpl: new ExplicitContext(),
  recorder: new BatchRecorder({
    logger: new HttpLogger({
      endpoint: `${ZIPKIN_ENDPOINT}/api/v2/spans`,
      jsonEncoder: jsonEncoder.JSON_V2,
    }),
  }),
  localServiceName: "date-service",
});

await kafkaToDatabase();

const app = express();

app.use(zipkinMiddleware({ tracer }));

app.use(
  promMid({
    metricsPath: "/metrics",
    collectDefaultMetrics: true,
    requestDurationBuckets: [0.1, 0.5, 1, 1.5],
    requestLengthBuckets: [512, 1024, 5120, 10240, 51200, 102400],
    responseLengthBuckets: [512, 1024, 5120, 10240, 51200, 102400],
    collectGCMetrics: true,
  })
);

app.get("/", function (req, res) {
  return res.send("Hello World from todo-persistent");
});

app.get("/todo", async function (req, res) {
  const todos = await fetchFromDatabase();

  return res.send(todos);
});

app.get("/metrics", async (req, res) => {
  res.set("Content-Type", promClient.register.contentType);
  res.end(await promClient.register.metrics());
});

app.listen(3000, function () {
  console.log("Listening on port 3000");
});
