import { Given } from "@cucumber/cucumber";
import { chromium } from "playwright/test";
import { url, context } from "./common.js";

Given("I am on the homepage", async () => {
  context.browser = await chromium.launch();
  context.page = await context.browser.newPage();
  await context.page.goto(url);
});

Given("another browser opens on the homepage", async () => {
  context.anotherBrowser = await chromium.launch();
  context.anotherPage = await context.anotherBrowser.newPage();
  await context.anotherPage.goto(url);
});
