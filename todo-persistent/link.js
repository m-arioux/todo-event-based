import { Kafka } from "kafkajs";
import { MongoClient } from "mongodb";

const mongoClient = new MongoClient(
  "mongodb://root:password@mongodb/main?authMechanism=SCRAM-SHA-1&authSource=admin"
);

const db = mongoClient.db("main");

const todos = db.collection("todo");

export async function kafkaToDatabase() {
  const kafka = new Kafka({
    clientId: "todo-persistent",
    brokers: ["kafka:9092"],
  });

  const consumer = kafka.consumer({ groupId: "todo-persistent" });

  await consumer.connect();

  // TODO: add index to save last handled todo
  await consumer.subscribe({ topic: "todo" });

  await consumer.run({
    eachMessage: async ({ topic, message }) => {
      console.log("received event", message.value.toString());

      if (topic != "todo") {
        return;
      }

      const todo = { id: message.key, description: message.value.toString() };

      todos.insertOne(todo);
    },
  });
}

export async function fetchFromDatabase() {
  return await todos
    .find()
    .map((x) => ({ id: x._id, description: x.description }))
    .toArray();
}
