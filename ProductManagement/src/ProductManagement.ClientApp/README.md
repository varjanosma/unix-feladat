# ProductManagement.ClientApp — Angular frontend

Angular **22.2** application, created with `ng new` and wired to the
`ProductManagement.Web` backend.

It intentionally contains no business features. The single page calls
`GET /api/health` and displays the result, which proves the whole chain works:
Angular → Web → Bll → Dal.

## Commands

| Command | What it does |
|---|---|
| `npm start` | dev server on http://localhost:4200, `/api` proxied to the backend |
| `npm run build` | production bundle into `../ProductManagement.Web/wwwroot` |
| `npm test` | unit tests (vitest + jsdom) |

`ProductManagement.Web` must be running on http://localhost:5080 for the health call
to succeed.

## How it is wired to the backend

- **`proxy.conf.json`** — during `ng serve`, `/api` and `/swagger` are forwarded to
  `http://localhost:5080`, so development makes same-origin requests and needs no CORS.
- **`src/environments/`** — `apiBaseUrl` is `''` in both configurations, because
  `ProductManagement.Web` serves the API and this bundle from the same origin, so every
  request is relative. Set an absolute URL here if the API ever moves to its own host.
- **`src/app/api/api-client.ts`** — **generated**, do not edit by hand. Regenerate from
  the solution root:

  ```bash
  dotnet nswag run nswag.json
  ```

## Build output

`angular.json` points `outputPath` at `../ProductManagement.Web/wwwroot`, so a
production build lands directly in the ASP.NET Core host's static file folder. That
folder is generated and is not in source control.
