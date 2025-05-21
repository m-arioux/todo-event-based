import { setDefaultTimeout } from "@cucumber/cucumber";

setDefaultTimeout(20_000);

import "./steps/common.js";
import "./steps/homepageSteps.js";
import "./steps/todoSteps.js";
import "./steps/hooks.js";
