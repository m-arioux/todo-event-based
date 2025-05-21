import { When, Then } from "@cucumber/cucumber";
import { expect } from "playwright/test";
import { faker } from "@faker-js/faker";
import { context, sleep } from "./common.js";

When("adding a todo", async () => {
  context.todoName = faker.string.alpha(15);
  await context.page.getByRole("textbox").fill(context.todoName);
  await context.page.getByRole("button", { name: "Send" }).click();
});

When("waiting for the todo to be saved", async () => {
  await sleep(3000);
});

Then("the todo is displayed", async () => {
  expect(context.todoName).toBeDefined();
  await expect(context.page.getByText(context.todoName)).toBeVisible();
});

Then("the field should clear", async () => {
  await expect(context.page.getByRole("textbox")).toBeEmpty();
});

Then("the todo is displayed on the other browser", async () => {
  expect(context.todoName).toBeDefined();
  await expect(context.anotherPage.getByText(context.todoName)).toBeVisible();
});
