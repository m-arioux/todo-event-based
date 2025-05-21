import { After } from "@cucumber/cucumber";
import { context } from "./common.js";

After(async () => {
  if (context.browser) await context.browser.close();
  if (context.anotherBrowser) await context.anotherBrowser.close();
});
