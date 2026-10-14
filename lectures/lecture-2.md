# Session 2 — C# in practice: data, errors, asynchrony

Each block: a short intro, then a task **in ProductManagement**. Try it in Swagger, then
walk through it together. The tasks add new services and endpoints; the existing ones stay
as they are. Reference solutions are published after the session.

**Reference solutions** (this branch):

| Task | Solution |
|---|---|
| 1 | [`PagedResultDto.cs`](../ProductManagement/src/ProductManagement.Transfer/Dtos/PagedResultDto.cs), [`ProductSearchRequest.cs`](../ProductManagement/src/ProductManagement.Transfer/Dtos/ProductSearchRequest.cs), [`ProductSort.cs`](../ProductManagement/src/ProductManagement.Transfer/Dtos/ProductSort.cs) |
| 2 | [`StockService.cs`](../ProductManagement/src/ProductManagement.Bll/Services/StockService.cs), [`StockController.cs`](../ProductManagement/src/ProductManagement.Web/Controllers/StockController.cs) |
| 3a | [`ProductSearchService.cs`](../ProductManagement/src/ProductManagement.Bll/Services/ProductSearchService.cs), [`ProductSearchController.cs`](../ProductManagement/src/ProductManagement.Web/Controllers/ProductSearchController.cs) |
| 3b | [`SalesStatisticsService.cs`](../ProductManagement/src/ProductManagement.Bll/Services/SalesStatisticsService.cs), [`StatisticsController.cs`](../ProductManagement/src/ProductManagement.Web/Controllers/StatisticsController.cs) |
| 4 | [`OrderService.cs`](../ProductManagement/src/ProductManagement.Bll/Services/OrderService.cs), [`Exceptions/`](../ProductManagement/src/ProductManagement.Bll/Exceptions/), [`OrdersController.cs`](../ProductManagement/src/ProductManagement.Web/Controllers/OrdersController.cs) |
| 5 | [`ExportService.cs`](../ProductManagement/src/ProductManagement.Bll/Services/ExportService.cs), [`InvoiceXml.cs`](../ProductManagement/src/ProductManagement.Bll/Export/InvoiceXml.cs), [`ExportController.cs`](../ProductManagement/src/ProductManagement.Web/Controllers/ExportController.cs) |
| 6 | [`FrankfurterRateProvider.cs`](../ProductManagement/src/ProductManagement.Bll/Rates/FrankfurterRateProvider.cs), [`PriceService.cs`](../ProductManagement/src/ProductManagement.Bll/Services/PriceService.cs), [`PricesController.cs`](../ProductManagement/src/ProductManagement.Web/Controllers/PricesController.cs) |
| 7 | [`tests/…/Lecture02/`](../ProductManagement/tests/ProductManagement.Bll.Tests/Lecture02/) |
| 8 | [`CheckoutService.cs`](../ProductManagement/src/ProductManagement.Bll/Services/CheckoutService.cs), `OrdersController.Checkout` |
| Homework | [`lecture-2-lab/`](lecture-2-lab/Program.cs), [`lecture-2-lab-2/`](lecture-2-lab-2/Program.cs) |

Registration of all new services:
[`ServiceCollectionExtensions.cs`](../ProductManagement/src/ProductManagement.Bll/Extensions/ServiceCollectionExtensions.cs).

---

## 0. Working in ProductManagement

### 0.1 Run it

```bash
cd ProductManagement
dotnet run --project src/ProductManagement.Web
```

Swagger UI: <http://localhost:5080/swagger>. Every new endpoint shows up there, with a
**Try it out** button. The database is in-memory: a restart resets the data.

Debug in VS Code: `F5` → choose **C#** and `ProductManagement.Web`; breakpoints with `F9`.

### 0.2 Where does new code go?

The same four steps for every task:

| Step | Project / folder | Example |
|---|---|---|
| 1. Data that goes in or out | `ProductManagement.Transfer/Dtos/` | `LowStockItemDto.cs` (a `record`) |
| 2. Contract + logic | `ProductManagement.Bll/Services/` | `IStockService.cs` (`public interface`) + `StockService.cs` (`internal sealed class`) |
| 3. Registration | `ProductManagement.Bll/Extensions/ServiceCollectionExtensions.cs` | `services.AddScoped<IStockService, StockService>();` |
| 4. HTTP endpoint | `ProductManagement.Web/Controllers/` | `StockController.cs` |

One type per file. Copy the shape from what's already there:
[`IProductService.cs`](../ProductManagement/src/ProductManagement.Bll/Services/IProductService.cs),
[`ProductService.cs`](../ProductManagement/src/ProductManagement.Bll/Services/ProductService.cs),
[`ProductsController.cs`](../ProductManagement/src/ProductManagement.Web/Controllers/ProductsController.cs).

```csharp
// Bll/Services/StockService.cs: the skeleton of every service
internal sealed class StockService : IStockService
{
    private readonly ProductManagementDbContext _dbContext;

    public StockService(ProductManagementDbContext dbContext)   // filled in by the framework
    {
        _dbContext = dbContext;
    }
}
```

New file in VS Code: right-click the folder → **New C#…** (C# Dev Kit) → *Class / Interface /
Record*. It fills in the namespace.

Tests: `dotnet test tests/ProductManagement.Bll.Tests` (or the **Testing** panel).

---

## 1. Modern C# building blocks

These four features appear on almost every line of LINQ and ASP.NET code, so they come
first.

### 1.1 Generics: write it once for any type

`List<T>` means "a list of *something*, fixed at compile time". The compiler checks
the type, so you can't add a `string` to a `List<Product>` by mistake (Harbour arrays
would accept it).

```csharp
// A tiny generic type: either a value or an error message
public sealed record Result<T>(T? Value, string? Error)
{
    public bool IsSuccess => Error is null;

    public static Result<T> Ok(T value)       => new(value, null);
    public static Result<T> Fail(string error) => new(default, error);
}

Result<decimal> price = Result<decimal>.Ok(1290m);
Result<Product> missing = Result<Product>.Fail("SKU not found");
```

### 1.2 Lambdas: a function as a value

A lambda is an inline, nameless function. It's the same idea as a Harbour code block
`{| p | p:Price > 100 }`.

```csharp
Func<Product, bool> isExpensive = p => p.Price > 100m;   // input → bool
Func<decimal, decimal> addVat   = net => net * 1.27m;     // input → output
Action<string> log              = msg => Console.WriteLine(msg); // no return value

bool b = isExpensive(product);
```

`Func<..., TResult>` returns a value, `Action<...>` doesn't. LINQ takes these as arguments.

The last type argument of `Func` is always the **return type**; the ones before it are the
parameters:

```csharp
Func<DateTime> now                    = () => DateTime.UtcNow;          // no input → DateTime
Func<int, decimal, decimal> lineTotal = (qty, price) => qty * price;    // 2 inputs → decimal
Action<string, int> report            = (sku, qty) => Console.WriteLine($"{sku}: {qty}");

decimal total = lineTotal(3, 45.50m);   // 136.50
report("TYR-001", 4);
```

A lambda can have a body with several statements, but then it needs `return`:

```csharp
Func<string, string> normalize = raw =>
{
    if (string.IsNullOrWhiteSpace(raw))
        return string.Empty;
    return raw.Trim().ToUpperInvariant();
};
```

A lambda can use local variables from around it (a **closure**). That's how a filter
picks up a value from the user:

```csharp
decimal limit = 1000m;                           // e.g. from a request parameter
Func<Product, bool> overLimit = p => p.Price > limit;

limit = 50m;                                     // captured by reference: the lambda sees 50 now
```

Passing a lambda to your own method is how LINQ works. Here's a simplified `Where`:

```csharp
static List<Product> Filter(List<Product> items, Func<Product, bool> predicate)
{
    var result = new List<Product>();
    foreach (var item in items)
        if (predicate(item))           // the caller decides what "matches" means
            result.Add(item);
    return result;
}

var expensive = Filter(products, p => p.Price > 1000m);
var scooters  = Filter(products, p => p.Name.Contains("Scooter"));
```

A typical use outside LINQ is passing in **behaviour**, e.g. a retry helper:

```csharp
static async Task<T> RetryAsync<T>(Func<Task<T>> operation, int attempts = 3)
{
    for (var i = 1; ; i++)
    {
        try { return await operation(); }
        catch (HttpRequestException) when (i < attempts) { await Task.Delay(500 * i); }
    }
}

var rate = await RetryAsync(() => exchangeClient.GetEurRateAsync());
```

(The last one uses `async`/`await` from section 6; come back to it there.)

### 1.3 Extension methods: "adding" a method to a type you don't own

```csharp
public static class StringExtensions
{
    // "this" on the first parameter makes it callable as sku.NormalizeSku()
    public static string NormalizeSku(this string sku) => sku.Trim().ToUpperInvariant();
}

var sku  = "  veh-001 ".NormalizeSku();                     // "VEH-001"
var same = StringExtensions.NormalizeSku("  veh-001 ");     // the very same call, written as a static method
```

It's still a plain `static` method; the call syntax is the only difference. **All of LINQ is
built from extension methods** on `IEnumerable<T>`.

### 1.4 Records and immutability

```csharp
public sealed record Product(int Id, string Sku, string Name, decimal Price);

var a = new Product(1, "VEH-001", "Scooter", 1290m);
var b = a with { Price = 1190m };     // a copy with one change; a is untouched

Console.WriteLine(a == b);            // False: records compare by VALUE, not by reference
Console.WriteLine(a);                 // Product { Id = 1, Sku = VEH-001, ... } for free
```

- **Immutable** = once created, it doesn't change. No "who modified this halfway through
  the procedure?" debugging.
- Use records for **data that travels**: DTOs, requests, results, JSON models.
- The property syntax `{ get; init; }` gives the same immutability with named properties.
  That's the style of the DTOs in the demo, e.g.
  [`ProductDetailDto.cs`](../ProductManagement/src/ProductManagement.Transfer/Dtos/ProductDetailDto.cs).

### Task 1: DTOs for the product search

In `ProductManagement.Transfer/Dtos/` (used in task 3):

- `PagedResultDto<T>`: a **generic** record with `Items` (`IReadOnlyList<T>`), `Page`,
  `PageSize`, `TotalCount`, and a computed `TotalPages`.
- `ProductSort` enum: `Name`, `PriceAscending`, `PriceDescending`, `StockAscending`.
- `ProductSearchRequest` record, every property `init` with a default: `ProductGroupId?`,
  `Search?`, `MinPrice?`, `MaxPrice?`, `ActiveOnly = true`, `Sort = Name`, `Page = 1`,
  `PageSize = 10`.

**Done when** it builds.

---

## 2. Collections

Session 1 covered them as a lab. The rule: **pick the collection by the question you ask it.**

| Question | Collection | Harbour equivalent |
|---|---|---|
| "Give me item #n", fixed size | `T[]` | array |
| "Keep these in order, add more" | `List<T>` | array + `AAdd` |
| "Find by key, fast" | `Dictionary<TKey, TValue>` | `hb_Hash()` |
| "Have I seen this already?" | `HashSet<T>` | hash with dummy values |
| "Hand this to the caller, read-only" | `IReadOnlyList<T>` / `IReadOnlyDictionary<,>` | — |

```csharp
var bySku = products.ToDictionary(p => p.Sku, StringComparer.OrdinalIgnoreCase);
if (bySku.TryGetValue("veh-001", out var car)) { ... }   // O(1), not a scan

var boughtIds = new HashSet<int> { 3, 6, 7 };
bool alreadyBought = boughtIds.Contains(product.Id);     // O(1)
```

`list.Contains(x)` inside a loop is a scan inside a scan, the `AScan`-in-a-loop problem.
Put the keys into a `HashSet` or `Dictionary` first.

### Task 2: Stock service

New `IStockService` / `StockService` + `StockController` (`api/stock`):

1. `GET /api/stock/low?threshold=10`: active products with `StockQuantity < threshold`,
   **keyed by product group name**: `IReadOnlyDictionary<string, IReadOnlyList<LowStockItemDto>>`.
   Load the rows with EF (`ToListAsync`), then build the dictionary with `TryGetValue`.
2. `GET /api/stock/recommendations/{userId}`: products from the groups the user already
   bought from, **that the user hasn't bought yet**, active and in stock, ordered by name.
   Two `HashSet`s: bought product ids, bought group ids. Unknown user → `null` → **404**.

**Done when:**

- `threshold=5` → `"Vehicles": [VEH-002, VEH-001]`, `"Engine and service": [ENG-003]`
- user `1` → `TYR-003`, `TYR-002`
- user `999` → 404

---

## 3. LINQ essentials

LINQ is a set of extension methods (section 1.3) that take lambdas (section 1.2) and work
on any collection, and on a database query too. Use **method syntax** only; ignore the
SQL-like query syntax.

```csharp
var expensive = products.Where(p => p.Price > 1000m);            // filter
var names     = products.Select(p => p.Name);                    // project
var first     = products.First(p => p.Sku == "VEH-001");         // throws if none
var maybe     = products.FirstOrDefault(p => p.Sku == "XXX");    // null if none
bool anyCheap = products.Any(p => p.Price < 10m);                // exists?
int count     = products.Count(p => p.Price > 1000m);
var sorted    = products.OrderBy(p => p.Name).ThenBy(p => p.Price);
var page2     = sorted.Skip(10).Take(10);                        // paging

var totalsByGroup = products
    .GroupBy(p => p.ProductGroupId)
    .Select(g => new { GroupId = g.Key, Count = g.Count(), Total = g.Sum(p => p.Price) });
```

### Three things to remember

1. **`First` vs `FirstOrDefault`.** `First` means "I *know* it's there", so a missing item
   throws. `FirstOrDefault` means "it may be missing", so check for `null`.
2. **Deferred execution.** `Where`/`Select` only *describe* the query. It runs when you
   iterate it or call `ToList()`, `Count()`, `First()`, and so on. Enumerating twice runs it
   twice.
3. **`Any()` instead of `Count() > 0`.** It stops at the first match.

| Harbour | LINQ |
|---|---|
| `AScan(a, {\| x \| ... })` | `FirstOrDefault(x => ...)` / `Any(x => ...)` |
| `AEval` + `AAdd` into a new array | `Select(...)` |
| loop + `IF` + `AAdd` | `Where(...)` |
| `ASort(a,,, {\| x, y \| ... })` | `OrderBy(...)` |

### Against the database: where does the filtering happen?

```csharp
// ❌ Works, but loads the WHOLE table into memory, then filters in C#
var all = await _dbContext.Products.ToListAsync(ct);
var cheap = all.Where(p => p.Price < 100m).Take(10).ToList();

// ✅ Build the query first; EF turns it into one SQL statement with WHERE / ORDER BY / OFFSET
var cheap = await _dbContext.Products
    .Where(p => p.Price < 100m)
    .OrderBy(p => p.Name)
    .Take(10)
    .Select(p => new ProductListItemDto { Id = p.Id, Name = p.Name, Price = p.Price })  // only the needed columns
    .ToListAsync(ct);
```

[`ProductService.GetListAsync`](../ProductManagement/src/ProductManagement.Bll/Services/ProductService.cs)
builds the query conditionally: `if (search) query = query.Where(...)`.

### Task 3a: Product search, written properly

New `IProductSearchService.SearchAsync(ProductSearchRequest)` → `PagedResultDto<ProductListItemDto>`,
endpoint `GET /api/product-search` with `[FromQuery] ProductSearchRequest request`.

- Every filter only when it's set (`ActiveOnly`, group, search text, min/max price).
- Sorting with a `switch` expression on `request.Sort`; add `ThenBy(p => p.Name)` for a stable order.
- `TotalCount` with `CountAsync` **before** paging, then `Skip` / `Take` / `Select` / `ToListAsync`.
- Clamp `PageSize` to 1–100: never trust the client with the size.

**Done when** `?minPrice=50&sort=PriceDescending&pageSize=2&page=2` → `TYR-003`, `BRK-001`,
`totalCount: 9`, `totalPages: 5`.

### Task 3b: Sales statistics

New `ISalesStatisticsService` + `StatisticsController` (`api/statistics`), both on
`PurchaseItems` with `GroupBy`:

- `GET /api/statistics/top-products?count=5`: quantity sold and revenue per product, by
  quantity descending, then revenue descending.
- `GET /api/statistics/revenue-by-group`: quantity and revenue per product group, highest
  revenue first.

**Done when** `count=2` → `TYR-001` (4 pcs, 436.00), `TYR-002` (4 pcs, 358.00), and the first
group is `Vehicles` with 18900.00.

---

## 4. Exceptions

```csharp
try
{
    var text = File.ReadAllText(path);
    return Parse(text);
}
catch (FileNotFoundException ex)          // most specific first
{
    throw new ImportException($"Input file missing: {path}", ex);  // keep the inner exception
}
finally
{
    // always runs: cleanup. For IDisposable prefer "using" instead.
}
```

A custom exception should carry **data**, not just text:

```csharp
public sealed class InsufficientStockException : Exception
{
    public InsufficientStockException(string sku, int requested, int available)
        : base($"Not enough stock for {sku}: requested {requested}, available {available}.")
    {
        Sku = sku;
        Requested = requested;
        Available = available;
    }

    public string Sku { get; }
    public int Requested { get; }
    public int Available { get; }
}
```

### What NOT to do

| Pattern | Problem |
|---|---|
| `catch (Exception) { }` | The error disappears. The data is wrong and nobody knows why. |
| `catch (Exception ex) { throw ex; }` | Resets the stack trace. Use `throw;` |
| `catch` → log → `return null` everywhere | The caller can't tell "not found" from "database down". |
| Exceptions for expected cases (bad input row) | Slow and noisy. Return a result or failure list instead. |

### The "catch → 200 OK, just an info message" pattern

```csharp
// ❌ Every failure looks like success to the client and to monitoring
public object GetProduct(int id)
{
    try
    {
        return new { ok = true, data = _service.Get(id) };
    }
    catch (Exception ex)
    {
        return new { ok = false, info = ex.Message };   // HTTP 200, internal details leaked
    }
}
```

```csharp
// ✅ Expected outcome → proper status code; unexpected → let it propagate
public async Task<ActionResult<ProductDetailDto>> GetById(int id, CancellationToken ct)
{
    var product = await _productService.GetByIdAsync(id, ct);
    if (product is null)
        return NotFound();          // 404: a normal, expected outcome
    return Ok(product);             // 200
    // A database failure is NOT caught here: it becomes a 500 on its own.
}
```

The Angular client, logs, monitoring and retries all decide based on the status code.
A 200 with `ok = false` forces every caller to re-implement error detection.

| Situation | Status code |
|---|---|
| OK / created | 200 / 201 |
| The request is wrong (validation) | 400 |
| No such thing | 404 |
| Valid request, but the current state doesn't allow it (no stock) | 409 |
| A service we depend on is down | 503 |
| A bug, an unexpected failure | 500 (not caught, happens on its own) |

### Task 4: Place an order

New `IOrderService.PlaceOrderAsync(PlaceOrderRequest)` → id of the new purchase, and
`OrdersController` with `POST /api/orders`.

Request DTOs (`Transfer`): `PlaceOrderRequest { UserId, Note?, Lines }`,
`OrderLineRequest { Sku, Quantity }`.

1. **Validate everything first, collect all errors** into a list: no lines, quantity ≤ 0,
   SKU listed twice, unknown user, unknown SKU, inactive product. If there's any, throw an
   `OrderValidationException` with an `Errors` list.
2. Then check the stock. Not enough → `InsufficientStockException(sku, requested, available)`.
3. **Only now change anything:** decrease the stock, create the `Purchase` with its
   `PurchaseItem`s (unit price = current product price), `SaveChangesAsync`.
4. Controller: catch **only these two** exceptions → 400 / 409 with the data in the body.
   Success → `CreatedAtAction(nameof(PurchasesController.GetById), "Purchases", new { id }, purchase)`.

Hint: load the ordered products in one query, into a `Dictionary` keyed by SKU (section 2).

**Done when:**

| Body | Expected |
|---|---|
| `{"userId":2,"lines":[{"sku":"TYR-001","quantity":2}]}` | 201, purchase with total 218.00; TYR-001 stock goes 64 → 62 |
| `{"userId":99,"lines":[{"sku":"NOPE","quantity":0}]}` | 400, three errors in the list |
| `{"userId":1,"lines":[{"sku":"VEH-002","quantity":9}]}` | 409, `"available": 2`, nothing changed |

---

## 5. Serialization

### 5.1 System.Text.Json

```csharp
using System.Text.Json;

var product = new Product(1, "VEH-001", "Scooter", 1290m);

string json = JsonSerializer.Serialize(product);
// {"Id":1,"Sku":"VEH-001","Name":"Scooter","Price":1290}

Product? back = JsonSerializer.Deserialize<Product>(json);
```

Options (create **once** and reuse, because building them is expensive):

```csharp
private static readonly JsonSerializerOptions JsonOptions = new()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,   // "sku" instead of "Sku"
    PropertyNameCaseInsensitive = true,                  // accept both when reading
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() },      // enums as "PriceAscending", not 1
};
```

Attributes, when the JSON doesn't match the C# names:

```csharp
public sealed record OrderLine(
    [property: JsonPropertyName("item_code")] string Sku,
    int Quantity,
    [property: JsonIgnore] decimal InternalCost);
```

**Bad input:** malformed JSON or a type mismatch (`"qty": "abc"`) throws `JsonException`,
with the path and line number in the message. Catch it **at the boundary** (where the file
or request comes in) and turn it into a meaningful error.

ASP.NET Core does this on every request and response (camelCase by default). That's
why the controllers return objects, not JSON strings.

### 5.2 XML, briefly

Relevant for the invoicing web service. `XmlSerializer` needs a **public class with a
parameterless constructor and settable properties**, so a positional record won't work here.

```csharp
using System.Xml.Serialization;

[XmlRoot("Invoice")]
public class InvoiceXml
{
    [XmlAttribute("number")] public string Number { get; set; } = "";
    [XmlElement("Total")]    public decimal Total  { get; set; }

    [XmlArray("Lines"), XmlArrayItem("Line")]
    public List<InvoiceLineXml> Lines { get; set; } = new();
}

var serializer = new XmlSerializer(typeof(InvoiceXml));   // create once, reuse
using var writer = new StringWriter();
serializer.Serialize(writer, new InvoiceXml { Number = "INV-1", Total = 1290m });
```

When a supplier provides an XSD, generate the classes (`xsd.exe` / `dotnet-svcutil`)
instead of writing them by hand.

### Task 5: Export

New `IExportService` + `ExportController` (`api/export`):

1. `GET /api/export/catalog`: the active products as a **downloadable JSON file**:
   camelCase, indented, enums as strings. In the controller: `File(bytes, "application/json", "catalog.json")`.
2. `GET /api/export/invoices/{purchaseId}`: the invoice of a purchase as **XML**,
   `File(bytes, "application/xml")`. Unknown purchase → 404. Reuse `IPurchaseService.GetByIdAsync`
   for the data.

Two small details make the XML clean: `XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) }`,
and an empty `XmlSerializerNamespaces` so there are no `xmlns:xsi` / `xmlns:xsd` attributes.

**Done when** purchase `1` gives:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Invoice number="INV-000001" currency="EUR">
  <IssueDate>2025-07-02</IssueDate>
  <Customer>
    <Name>Novák János</Name>
    <Email>janos.novak@example.com</Email>
  </Customer>
  <Lines>
    <Line sku="TYR-001">
      <Description>Summer tyre 205/55 R16</Description>
      <Quantity>4</Quantity>
      <UnitPrice>109.00</UnitPrice>
      <LineTotal>436.00</LineTotal>
    </Line>
    <Line sku="ENG-001">
      ...
    </Line>
  </Lines>
  <Total>448.90</Total>
</Invoice>
```

---

## 6. Async/await

### 6.1 What it solves

Async is about **not blocking a thread while waiting for I/O**: disk, network, database.
It is **not** about running things in parallel.

> A waiter takes an order to the kitchen and serves other tables while it cooks. They
> don't stand in the kitchen door waiting. There is still one waiter, and nothing happens
> "in parallel". The waiter just isn't idle.

A web server has a limited pool of threads. If each request blocks a thread while waiting
for SQL, the server runs out of threads long before it runs out of CPU.

### 6.2 The syntax

```csharp
// Synchronous: the thread waits
string text = File.ReadAllText(path);

// Asynchronous: the thread is released while the disk works
string text = await File.ReadAllTextAsync(path);
```

- `Task` = "an operation that finishes later". `Task<T>` = "…and then gives a `T`".
- `async` on the method allows `await` inside it.
- `await` = "continue from here when it's done; release the thread meanwhile".
- Naming convention: the method name ends in `Async`.

### 6.3 It spreads upwards

As soon as one method `await`s, its caller must `await` it too, and so on up to the top:

```text
Controller.GetList      async Task<ActionResult<...>>   await ↓
  ProductService.GetListAsync   async Task<IReadOnlyList<...>>   await ↓
    EF ToListAsync      ← the actual I/O
```

This is by design. **Async all the way.** Don't stop halfway with `.Result`.

### 6.4 Typical mistakes

| Mistake | What happens | Instead |
|---|---|---|
| `.Result` / `.Wait()` / `.GetAwaiter().GetResult()` | Blocks the thread; can deadlock; exceptions arrive wrapped in `AggregateException` | `await` |
| `async void` method | Caller can't await it; an exception can crash the process | `async Task` (the only exception: UI event handlers) |
| Forgetting `await` | Method returns before the work is done; exceptions are lost | Compiler warning CS4014: treat it as an error |
| `Task.Run(() => File.ReadAllText(...))` in a web app | Burns a second thread to wait on I/O | Use the real `...Async` API |
| Not passing the `CancellationToken` | Work continues after the client has left | Pass it all the way down |

### 6.5 CancellationToken

The way to say "stop, nobody needs the result any more", e.g. when the user closes the
browser tab during a 30-second report. ASP.NET fills in the token of the request:

```csharp
[HttpGet]
public async Task<ActionResult<IReadOnlyList<ProductListItemDto>>> GetList(
    [FromQuery] string? search, CancellationToken cancellationToken)
{
    var products = await _productService.GetListAsync(null, search, cancellationToken);
    return Ok(products);
}
```

The rule: **accept a `CancellationToken` as the last parameter and pass it to every async
call you make.** The libraries (EF, HttpClient, file APIs) do the actual cancelling.

A **timeout** is just a cancellation. To add your own limit on top of the caller's token:

```csharp
using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
timeout.CancelAfter(TimeSpan.FromSeconds(5));       // whichever fires first wins

try
{
    return await _http.GetFromJsonAsync<RateResponse>(url, timeout.Token);
}
catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
{
    // OUR timeout fired, not the client's: that's a failure, not a cancellation
    throw new ExchangeRateUnavailableException("The rate service did not answer in 5 seconds.");
}
```

### Task 6: Prices in another currency

Product prices are stored in EUR. New endpoint `GET /api/prices/{productId}?currency=HUF`
with today's ECB rate from a free API (no key needed):

```text
GET https://api.frankfurter.dev/v1/latest?base=EUR&symbols=HUF
→ { "amount": 1.0, "base": "EUR", "date": "2026-10-14", "rates": { "HUF": 363.3 } }
```

1. `IExchangeRateProvider.GetRateAsync(from, to, ct)` → `ExchangeRate(From, To, Rate, Date)` record,
   in `Bll/Rates/`.
2. `FrankfurterRateProvider`: gets an `HttpClient` in the constructor, calls
   `GetFromJsonAsync`, has its own **5-second timeout** (snippet above). Translates the
   failures into **our own** exceptions:
   - API answers 404 (`HttpRequestException` with `StatusCode == NotFound`) → `UnknownCurrencyException`
   - any other `HttpRequestException` / `JsonException` / our timeout → `ExchangeRateUnavailableException`
3. Register it as a singleton, with one `HttpClient` for the whole application:

   ```csharp
   services.AddSingleton<IExchangeRateProvider>(_ => new FrankfurterRateProvider(
       new HttpClient { BaseAddress = new Uri("https://api.frankfurter.dev/v1/") }));
   ```

4. `IPriceService.GetPriceAsync(productId, currency)` → `ProductPriceDto?`: `await` the
   database, then `await` the rate. **EUR → no HTTP call at all.** Round with
   `Math.Round(x, 2, MidpointRounding.AwayFromZero)`.
5. `PricesController`: no product → 404, `UnknownCurrencyException` → 400,
   `ExchangeRateUnavailableException` → 503.

**Done when:** `1?currency=HUF` → the Golf in forints; `currency=XXX` → 400; `currency=EUR` → no
call to the API (breakpoint!); `999` → 404. Try `TimeSpan.FromMilliseconds(1)` as the timeout → 503.

---

## 7. Interfaces and tests

An interface says **what** something can do, not **how**. `PriceService` only knows
`IExchangeRateProvider`; it has no idea there's an HTTP call behind it:

```csharp
internal sealed class PriceService : IPriceService
{
    private readonly IExchangeRateProvider _rateProvider;   // the contract, not FrankfurterRateProvider

    public PriceService(ProductManagementDbContext dbContext, IExchangeRateProvider rateProvider) { ... }
}
```

- The **interface is `public`**, the **implementation is `internal sealed`** (session 1):
  other projects see the contract, never the details.
- Which implementation is used is decided in one place: `ServiceCollectionExtensions`.
- And a test can pass in a **fake**: no network, same answer every time.

```csharp
internal sealed class FakeExchangeRateProvider : IExchangeRateProvider
{
    public bool IsDown { get; init; }
    public int CallCount { get; private set; }

    public Task<ExchangeRate> GetRateAsync(string from, string to, CancellationToken ct = default)
    {
        CallCount++;
        if (IsDown) throw new ExchangeRateUnavailableException("Fake: down.");
        return Task.FromResult(new ExchangeRate(from, to, 400m, "2026-01-01"));
    }
}

public class PriceServiceTests
{
    [Fact]
    public async Task Price_is_converted_with_the_rate_of_the_provider()
    {
        // Arrange
        using var db = TestDb.Create();
        var service = new PriceService(db, new FakeExchangeRateProvider());

        // Act
        var price = await service.GetPriceAsync(productId: 6, currency: "HUF");   // oil filter, 12.90 EUR

        // Assert
        Assert.Equal(5_160m, price!.Price);
    }
}
```

- xUnit: `[Fact]` = one test, `Assert.*` = the check.
- Hand-written fakes are enough to start with. **NSubstitute / Moq** generate them.

### Task 7: Unit tests

In `tests/ProductManagement.Bll.Tests/Lecture02/`:

1. The services are `internal`, so let the test project see them, in `ProductManagement.Bll.csproj`:

   ```xml
   <ItemGroup>
     <InternalsVisibleTo Include="ProductManagement.Bll.Tests" />
   </ItemGroup>
   ```

2. A fresh in-memory database with the seed data for **every** test, so tests can't affect each other:

   ```csharp
   internal static class TestDb
   {
       public static ProductManagementDbContext Create()
       {
           var options = new DbContextOptionsBuilder<ProductManagementDbContext>()
               .UseInMemoryDatabase(Guid.NewGuid().ToString())
               .Options;

           var dbContext = new ProductManagementDbContext(options);
           dbContext.Database.EnsureCreated();
           return dbContext;
       }
   }
   ```

3. Tests to write:
   - price conversion with the fake (above);
   - `currency: "EUR"` with `IsDown = true` still works, and `CallCount` is 0;
   - placing an order with too much quantity throws `InsufficientStockException` **and
     changes nothing** (purchase count and stock are the same as before);
   - your choice: one for task 2 or 3.

**Done when** `dotnet test tests/ProductManagement.Bll.Tests` is green.

---

## 8. Combined task: checkout

Walk through this one together. It uses everything above.

`POST /api/orders/checkout?currency=HUF`, body: a `PlaceOrderRequest` (task 4). Response:

```json
{
  "purchaseId": 7,
  "totalEur": 12.90,
  "currency": "HUF",
  "rate": 363.3,
  "total": 4686.57,
  "invoiceUrl": "/api/export/invoices/7"
}
```

1. New `ICheckoutService` / `CheckoutService`. It gets `IOrderService`, `IPurchaseService` and
   `IExchangeRateProvider` in the constructor: **it composes services**, it doesn't
   re-implement them.
2. Order of steps: **get the rate first, then place the order.** If the rate service is
   down, no order may be created.
3. Total = purchase total × rate, rounded. `invoiceUrl` points to the export from task 5.
4. Controller: 200 / 400 (validation, unknown currency) / 409 (stock) / 503 (rates).
5. Test: with `FakeExchangeRateProvider { IsDown = true }` the call throws and the number of
   purchases doesn't change.

Then follow one request in the debugger, end to end:

```text
Swagger → OrdersController.Checkout → CheckoutService
            → IExchangeRateProvider (HTTP)  → OrderService (EF → in-memory DB)
            → PurchaseService → back as CheckoutResultDto → JSON
```

Discuss: what happens with each failure in the chain, and where it turns into a status code?

---

## 9. Homework (optional)

Two small console apps, outside ProductManagement, for anyone who wants more practice.

**Setup:**

```bash
dotnet new console -n OrdersLab && cd OrdersLab && dotnet run
```

Put the input file next to the exe, in the `.csproj`:

```xml
<ItemGroup>
  <None Update="orders.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

…and read it with `Path.Combine(AppContext.BaseDirectory, "orders.json")`. Folders by role
(`Models/`, `Sources/`, `Processing/`), `Program.cs` only wires things together.

### Lab A: JSON in → typed objects → JSON out

```json
[
  { "orderId": "A-1", "sku": "VEH-001", "quantity": 2, "unitPrice": 1290.00 },
  { "orderId": "A-2", "sku": "TYR-001", "quantity": 4, "unitPrice": 45.50 },
  { "orderId": "A-3", "sku": "",        "quantity": 1, "unitPrice": 10.00 },
  { "orderId": "A-4", "sku": "VEH-001", "quantity": -3, "unitPrice": 1290.00 },
  { "orderId": "A-5", "sku": "BRK-010", "quantity": 1, "unitPrice": 89.90 }
]
```

1. Read the file **asynchronously**, behind an `IOrderSource` interface.
2. Empty SKU or quantity ≤ 0 → a failure list (an expected case: no exception).
3. Valid orders per SKU with `GroupBy` + `Sum`, by revenue descending.
4. Write `summary.json`: `{ "totals": [...], "failures": [...] }`.
5. Missing file / bad JSON: not swallowed; wrapped in a custom exception with the path.

### Lab B: Price list with live rates

Same shape, HUF prices from `prices.json`, converted to EUR and USD with
`https://api.frankfurter.dev/v1/latest?base=HUF&symbols=EUR,USD`. A 5-second timeout on
the whole run, a fake provider for offline use (`dotnet run -- offline`), and every failure
(404, no network, bad JSON, timeout) ends in a clear message and a non-zero exit code.

---

## 10. What to take away

- Records for data, `IReadOnlyList<T>` for return values: immutable by default.
- Pick the collection by the question: key lookup → `Dictionary`, membership → `HashSet`.
- LINQ = extension methods + lambdas. Against the database: filter and page **before** `ToListAsync`.
- Never swallow exceptions. Expected outcomes are results or status codes; unexpected ones propagate.
- An error is never a `200 OK` with an info message.
- `System.Text.Json` for JSON; catch `JsonException` at the boundary. XML only where an external system requires it.
- Async is for I/O, not parallelism. `await` all the way up; no `.Result`, no `async void`.
- Accept a `CancellationToken` and pass it down. A timeout is just a cancellation.
- Depend on interfaces; that's what makes code swappable and testable.
