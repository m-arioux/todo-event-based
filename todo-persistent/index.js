import express from "express";
import { fetchFromDatabase, kafkaToDatabase } from "./link.js";
import promClient from "prom-client";

promClient.collectDefaultMetrics();

await kafkaToDatabase();

const app = express();

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
