db.createUser({
  user: "user",
  pwd: "password",
  roles: [
    {
      role: "readWrite",
      db: "main",
    },
  ],
});

db.createUser({
  user: "prometheus",
  pwd: "9081u43981eh",
  roles: [{ role: "readWrite", db: "main" }],
});

db.createCollection("todo");
