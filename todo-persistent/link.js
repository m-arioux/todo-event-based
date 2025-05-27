import { Kafka } from "kafkajs";
import { MongoClient } from "mongodb";
import instrumentKafkaJs from "zipkin-instrumentation-kafkajs";

const mongoClient = new MongoClient(
  "mongodb://root:password@mongodb/main?authMechanism=SCRAM-SHA-1&authSource=admin"
);

const db = mongoClient.db("main");

const todos = db.collection("todo");

export async function kafkaToDatabase(tracer) {
  const kafka = instrumentKafkaJs(
    new Kafka({
      clientId: "todo-persistent",
      brokers: ["kafka:9092"],
    }),
    { tracer, remoteServiceName }
  );

  const consumer = kafka.consumer({
    groupId: "todo-persistent",
    readUncommitted: false,
  });

  await consumer.connect();

  // TODO: add index to save last handled todo
  await consumer.subscribe({ topic: "todo" });

  await consumer.run({
    autoCommit: false,
    eachMessage: async ({ topic, message }) => {
      console.log("received event", message.value.toString());

      if (topic != "todo") {
        await consumer.commitOffsets();
        return;
      }

      const todo = { id: message.key, description: message.value.toString() };

      await todos.insertOne(todo);

      await consumer.commitOffsets();
    },
  });
}

export async function fetchFromDatabase() {
  return await todos
    .find()
    .map((x) => ({ id: x._id, description: x.description }))
    .toArray();
}
