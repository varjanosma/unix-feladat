# ProductManagement — demo solution

Teaching skeleton for the .NET course: **.NET 6 backend + Angular 22 frontend**, with a
small but complete domain on top of it: a car webshop that sells used cars, parts and
accessories.

The point of the business logic is not the business logic. It exists so that the
request path everybody will actually work with is visible end to end:

```
Angular page  ->  Controller  ->  Service (Bll)  ->  EF Core (Dal)  ->  database
```

## Layout

```
ProductManagement.sln
├─ global.json                   SDK pin (.NET 6.0.4xx)
├─ Directory.Build.props         shared settings (TFM, nullable, implicit usings)
├─ nswag.json                    OpenAPI -> Angular TypeScript client generation
├─ ProductManagement.slnLaunch   Visual Studio startup profiles
├─ docs/domain-model.md          class diagram and the modelling decisions
└─ src/
   ├─ ProductManagement.Web        controllers, NSwag, serves the SPA
   ├─ ProductManagement.Bll        services (business logic)
   ├─ ProductManagement.Transfer   DTOs - the API contract, no dependencies
   ├─ ProductManagement.Dal        entities, EF Core mapping, Seed/SeedData.cs
   └─ ProductManagement.ClientApp  the Angular application
```

Dependency direction (arrows point one way only):

```
ClientApp (Angular)  ->  Web  ->  Bll  ->  Dal

                 Web and Bll both reference
                 ProductManagement.Transfer (DTOs)
```

`Web` does not reference `Dal` directly. Each layer brings its own DI registration:
`AddBll()` calls `AddDal()` internally. Entities never leave Dal/Bll — the controllers
hand out **DTOs** from `ProductManagement.Transfer`, and the Angular client is
generated from that contract.

**`ProductManagement.Web` is the single startup project.** It hosts the API, generates
the OpenAPI document with NSwag, and serves the compiled Angular bundle from `wwwroot`,
so the browser talks to one origin and CORS is not involved.

## Prerequisites

- **.NET 6 SDK (6.0.4xx)** — check with `dotnet --version` → `6.0.428`
- **Node.js 22.22+ or 24.15+** — required by Angular 22 (`node --version`)

## Running it

### Everything in one process

```bash
dotnet build                                    # first build also builds the Angular app
dotnet run --project src/ProductManagement.Web
```

- App: http://localhost:5080
- Swagger UI: http://localhost:5080/swagger

This serves the **compiled** Angular bundle. After changing frontend code, rebuild it
with `npm run build` in `ProductManagement.ClientApp`, or use the dev server below.

### With the Angular dev server (hot reload)

Two terminals:

```bash
# 1) backend + API
dotnet run --project src/ProductManagement.Web

# 2) frontend
cd src/ProductManagement.ClientApp && npm start
```

Browser: http://localhost:4200. `proxy.conf.json` forwards `/api` and `/swagger` to
port 5080, so this is same-origin as well.

### From Visual Studio

`ProductManagement.Web` is the startup project; F5 just works.
`ProductManagement.slnLaunch` adds two profiles to the dropdown next to the Start
button: **Web** (opens the app) and **Web (Swagger)** (opens Swagger UI).

Visual Studio does not start the Angular dev server. For hot reload, run `npm start`
in `ProductManagement.ClientApp` alongside, and use http://localhost:4200.

## The domain

Five entities: `ProductGroup`, `Product`, `ProductInfo`, `User`, `Purchase` and
`PurchaseItem`. Class diagram, the relationship types and the modelling decisions:
**[`docs/domain-model.md`](docs/domain-model.md)**.

The database is **EF Core InMemory** and is seeded from `Dal/Seed/SeedData.cs` on every start
(5 groups, 12 products, 3 customers, 5 purchases), so nothing external is needed to run it.
The entities themselves stay generic - `Product`, `ProductGroup`, `ProductInfo` - only
the data is about cars.

### API

| Endpoint | What it returns |
|---|---|
| `GET /api/products?productGroupId=&search=` | product list, optionally filtered |
| `GET /api/products/{id}` | one product with its `ProductInfo` details, or **404** |
| `GET /api/product-groups` | groups with the number of products in each |
| `GET /api/purchases?userId=` | purchases, newest first, optionally one customer |
| `GET /api/purchases/{id}` | one purchase with buyer and lines, or **404** |
| `GET /api/users` | customers with purchase count and total spent |
| `GET /api/users/{id}` | one customer with the full purchase history, or **404** |
| `GET /api/health` | layer/DI health check |

Every endpoint is `async` and takes a `CancellationToken`, which the controller passes
down to EF Core.

### Angular pages

| Route | Page |
|---|---|
| `/products` | list with group filter and search |
| `/products/:id` | product details, handles 404 separately from a failed request |
| `/product-groups` | groups with product counts |
| `/purchases` | all purchases with a customer filter and a grand total |
| `/purchases/:id` | one purchase with its lines |
| `/users` | customers with aggregated numbers |
| `/users/:id` | customer with purchase history, grouped by purchase |
| `/health` | backend health check |

## Things worth pointing at during the course

- **DTO vs entity.** `ProductDetailDto` is not `Product`, and it lives in its own
  assembly with **zero dependencies** - so an entity, EF Core or business logic
  cannot accidentally leak into the API contract. Renaming a column does not break
  the contract, and the client only receives what it needs.
- **Projection instead of loading.** The services `Select` into DTOs inside the
  `IQueryable`, so only the needed columns are read. `AsNoTracking` because these are
  read-only queries.
- **404 is not 200.** `GetByIdAsync` returns `null`, the controller turns that into a
  `NotFound()`, and the Angular page tells "not found" apart from "request failed".
  This is the opposite of the "error + 200 OK with an info message" pattern.
- **`internal` has teeth.** The service implementations (`ProductService`, …) are
  `internal`; only the interfaces are `public`. The Web project physically cannot
  reach the implementation.
- **Price snapshot.** `PurchaseItem.UnitPrice` is a copy, not a reference to
  `Product.Price` — see `docs/domain-model.md`.
- **Unit tests.** `product-list.spec.ts` and `purchase-list.spec.ts` render the pages
  against a mocked HTTP backend; `npm test` runs them.

## Generating the Angular API client (NSwag)

```bash
dotnet tool restore          # once, based on .config/dotnet-tools.json
dotnet nswag run nswag.json
```

This builds `ProductManagement.Web`, reads the OpenAPI document out of it, and
generates `src/ProductManagement.ClientApp/src/app/api/api-client.ts`. That file is
generated — never edit it by hand; change the controller and regenerate.

Three details worth knowing:

- **NSwag 14.0.8** is the last release that still ships a `net6.0` toolchain, so it is
  the newest version usable here.
- **`typeScriptVersion: 5.0`** — without it NSwag emits pre-4.3 TypeScript and the
  generated `ApiException` fails to compile under Angular 22 with
  `TS4114: This member must have an 'override' modifier`.
- The generated interfaces mark **every property optional** (`id?: number`), because the
  OpenAPI document does not declare them required. That is why the templates use
  `?.` and `?? []` in places. Adding `[Required]` to the DTO properties would tighten it.

> Note: `ProductManagement.Web` references `Microsoft.AspNetCore.Mvc.NewtonsoftJson`
> even though the app itself uses `System.Text.Json`. The NSwag CLI document
> generator loads that assembly while inspecting the app, and generation fails
> without it.

## Frontend / backend wiring

Both the API and the Angular bundle are served by `ProductManagement.Web`, so
`apiBaseUrl` in `src/environments/` is empty — every request is relative and
same-origin. `ng serve` keeps that property through `proxy.conf.json`.

`ProductManagement.Web/wwwroot` is the Angular build output and is **not** in source
control. The Web project builds the client automatically when `wwwroot/index.html` is
missing (first build after a clone). Before `dotnet publish`, run `npm run build` in
`ProductManagement.ClientApp` — MSBuild collects `wwwroot` when the project is
evaluated, so files created during the same build are not picked up for publishing.

## A note on .NET 6

.NET 6 has been **out of support since 2024-11-12**. This demo targets it because
most of the current TL codebase sits there. The version upgrade is worth covering
as its own topic during the course — see [`../dotnet-into.md`](../dotnet-into.md).

The target framework is set in exactly one place (`Directory.Build.props`), so
moving the whole solution to .NET 8 or .NET 10 is a one-line change plus a matching
`global.json` update. Angular is not coupled to the .NET version at all — its real
constraint is the Node.js version.
