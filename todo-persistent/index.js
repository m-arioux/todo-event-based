import express from "express";
import { fetchFromDatabase, kafkaToDatabase } from "./link.js";

await kafkaToDatabase();

const app = express();

app.get("/", function (req, res) {
  return res.send("Hello World from todo-persistent");
});

app.get("/todo", async function (req, res) {
  const todos = await fetchFromDatabase();

  return res.send(todos);
});

app.listen(3000, function () {
  console.log("Listening on port 3000");
});
