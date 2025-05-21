# todo-event-based

A simple todo app using Apache Kafka with React, .NET, Express, MongoDB.
The purpose for making this is to get practical experience with Kafka and Docker/Podman.

# Getting Started

This was made using Podman, not Docker. It should work with Docker but Podman's architecture is simpler than Docker so you may encounter some network issues.

First, install Docker and Docker Compose (or Podman with Compose).

Then, only if using Docker, edit the `setup.sh` file and replace all `podman` commands for `docker`. The commands are supposed to be Docker-compatible.

Finally, simply run the `setup.sh` script and everything should build and then run.
On Windows, run it with `Git Bash`.

# Architecture

Here is a overview of every component and how they communicate between each others
![overview](/docs/overview.png)

SSE (Server-Side Events) is used between todo-spa and todo-service to be notified when a new todo is available.
All the events in the Kafka server are consumed by todo-persistent and todo-service. The first stores the todos in a monbodb database and the other uses SSE to send then to todo-spa.
todo-service is the only producer of events.

In a real world use case I would have made only one back-end accessible from the SPA for simplicity. When the SSE connection open I would send all previous todos in the connection before sending new todos. I did differently because I wanted to experiment a little with Next.JS.

## todo-spa

A React app with Next.JS accessible from localhost:3001

Its only purposes are to push new todos, show the todos in the database and the new ones created after the page loaded.

A reverse-proxy is configured to redirect requests.

- /api/service/\* redirected to todo-service
- /api/persistent/\* redirected to todo-persistent

This reverse-proxy let us not having to handle any CORS related issues.

## todo-service

A .NET webapi accessible from localhost:5000 having 2 endpoints:

- POST /todo to create a todo (Producer)
- GET /todo-live to open a SSE stream of todos (Consumer)

## todo-persistent

A NodeJS HTTP server accessible from localhost:3000, using Express.

GET /todo is the only endpoint. It gives all the todos saved into the MongoDB database.

It consumes all the todos from the topic and saves them in the database.

## todo-test

Some e2e tests using cucumber.js and playwright. You can run them by firstly installing dependencies with `npm i` and then `npm run test`

# Known issues

There's a flaky test.
