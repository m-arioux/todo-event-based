export const url = "http://localhost:3001";

export const context = {
  browser: null,
  page: null,
  anotherBrowser: null,
  anotherPage: null,
  todoName: null,
};

export function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}
