"use client";

import Image from "next/image";
import { FormEvent, useEffect, useState } from "react";

interface Todo {
  description: string;
  id: string;
}

export default function Home() {
  const [todos, setTodos] = useState([] as Todo[]);
  const [newTodo, setNewTodo] = useState("");

  useEffect(() => {
    fetch("/api/persistent/todo")
      .then((response) => response.json())
      .then((response) => {
        setTodos((previous) => [...previous, ...response]);
      });
  }, []);

  useEffect(() => {
    const eventSource = new EventSource("/api/service/todo-live");

    eventSource.addEventListener("message", (event) => {
      const todo = JSON.parse(event.data) as Todo;

      console.log("received ", todo);

      setTodos((previous) => previous.concat(todo));
    });

    return () => {
      eventSource.close();
    };
  }, []);

  const saveNewTodo = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    fetch("/api/service/todo", {
      body: JSON.stringify({ description: newTodo }),
      method: "POST",
      headers: { "Content-Type": "application/json" },
    }).then(() => {
      setNewTodo("");
    });
  };

  return (
    <div className="grid grid-rows-[20px_1fr_20px] items-center justify-items-center min-h-screen p-8 pb-20 gap-16 sm:p-20 font-[family-name:var(--font-geist-sans)]">
      <main className="flex flex-col gap-[32px] row-start-2 items-center sm:items-start">
        <Image
          className="dark:invert"
          src="/next.svg"
          alt="Next.js logo"
          width={180}
          height={38}
          priority
        />

        <h2>TODOS:</h2>
        <ul>
          {todos.map((x) => (
            <li key={x.id} className="items-center">
              {x.description}
            </li>
          ))}
        </ul>

        <form onSubmit={saveNewTodo}>
          <input
            type="text"
            value={newTodo}
            onChange={(e) => setNewTodo(e.target.value)}
            className="bg-gray-700 text-white"
            name="new-todo"
          ></input>
          <button type="submit" className="bg-green-800">
            Send
          </button>
        </form>
      </main>
      <footer className="row-start-3 flex gap-[24px] flex-wrap items-center justify-center">
        <a
          className="flex items-center gap-2 hover:underline hover:underline-offset-4"
          href="https://nextjs.org/learn?utm_source=create-next-app&utm_medium=appdir-template-tw&utm_campaign=create-next-app"
          target="_blank"
          rel="noopener noreferrer"
        >
          <Image
            aria-hidden
            src="/file.svg"
            alt="File icon"
            width={16}
            height={16}
          />
          Learn
        </a>
        <a
          className="flex items-center gap-2 hover:underline hover:underline-offset-4"
          href="https://vercel.com/templates?framework=next.js&utm_source=create-next-app&utm_medium=appdir-template-tw&utm_campaign=create-next-app"
          target="_blank"
          rel="noopener noreferrer"
        >
          <Image
            aria-hidden
            src="/window.svg"
            alt="Window icon"
            width={16}
            height={16}
          />
          Examples
        </a>
        <a
          className="flex items-center gap-2 hover:underline hover:underline-offset-4"
          href="https://nextjs.org?utm_source=create-next-app&utm_medium=appdir-template-tw&utm_campaign=create-next-app"
          target="_blank"
          rel="noopener noreferrer"
        >
          <Image
            aria-hidden
            src="/globe.svg"
            alt="Globe icon"
            width={16}
            height={16}
          />
          Go to nextjs.org →
        </a>
      </footer>
    </div>
  );
}
