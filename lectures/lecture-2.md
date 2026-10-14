# Session 2 — C# in practice: data, errors, asynchrony

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

**In ProductManagement:** [`ProductDetailDto.cs`](../ProductManagement/src/ProductManagement.Transfer/Dtos/ProductDetailDto.cs),
a `record` with `init`-only properties.

---

## 2. Collections: recap

Session 1 covered this as a lab ([`07_Collections.cs`](../ProductManagement/src/ProductManagement.Bll/Lectures01/07_Collections.cs)).
Today we only need the decision rule: **pick the collection by the question you ask it.**

| Question | Collection | Harbour equivalent |
|---|---|---|
| "Give me item #n", fixed size | `T[]` | array |
| "Keep these in order, add more" | `List<T>` | array + `AAdd` |
| "Find by key, fast" | `Dictionary<TKey, TValue>` | `hb_Hash()` |
| "Have I seen this already?" | `HashSet<T>` | hash with dummy values |
| "Hand this to the caller, read-only" | `IReadOnlyList<T>` / `IReadOnlyDictionary<,>` | — |

```csharp
var bySku = products.ToDictionary(p => p.Sku, StringComparer.OrdinalIgnoreCase);
if (bySku.TryGetValue("veh-001", out var scooter)) { ... }   // O(1), not a scan
```

Return `IReadOnlyList<T>` from methods: the caller can't modify your internal list.
Every service in the demo does this.

---

## 3. LINQ essentials

LINQ is a set of extension methods (section 1.3) that take lambdas (section 1.2) and work
on any collection, and on a database query too. Use **method syntax** only; ignore the
SQL-like query syntax.

```csharp
var products = new List<Product> { /* ... */ };

var expensive = products.Where(p => p.Price > 1000m);            // filter
var names     = products.Select(p => p.Name);                    // project
var first     = products.First(p => p.Sku == "VEH-001");         // throws if none
var maybe     = products.FirstOrDefault(p => p.Sku == "XXX");    // null if none
bool anyCheap = products.Any(p => p.Price < 10m);                // exists?
int count     = products.Count(p => p.Price > 1000m);
var sorted    = products.OrderBy(p => p.Name).ThenBy(p => p.Price);

// Chained: the typical shape
List<string> result = products
    .Where(p => p.Price > 1000m)
    .OrderBy(p => p.Name)
    .Select(p => $"{p.Sku} - {p.Name}")
    .ToList();
```

`GroupBy`, briefly:

```csharp
var totalsByGroup = products
    .GroupBy(p => p.GroupId)
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

**In ProductManagement:** [`ProductService.cs`](../ProductManagement/src/ProductManagement.Bll/Services/ProductService.cs)
builds the query up conditionally (`if (search) query = query.Where(...)`), and nothing
hits the database until `ToListAsync`. It's the same LINQ, but there EF translates it to SQL.

---

## 4. Exceptions

Recap of session 1 ([`08_ErrorHandling.cs`](../ProductManagement/src/ProductManagement.Bll/Lectures01/08_ErrorHandling.cs)),
then the one pattern that matters most for the rewrite.

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

```csharp
using var stream = File.OpenRead(path);   // disposed at the end of the scope, even on exception
```

A custom exception should carry **data**, not just text:

```csharp
public sealed class ProductNotFoundException : Exception
{
    public ProductNotFoundException(string sku) : base($"Product '{sku}' was not found.")
        => Sku = sku;

    public string Sku { get; }
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

This is the shape we want to get rid of:

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
    // A database failure is NOT caught here: one central handler turns it into a 500.
}
```

Why it matters: the Angular client, logs, monitoring and retries all decide based on the
status code. A 200 with `ok = false` forces every caller to re-implement error detection.

**In ProductManagement:** [`ProductsController.cs`](../ProductManagement/src/ProductManagement.Web/Controllers/ProductsController.cs) (`GetById`).
The central handler (error middleware + `ProblemDetails`) is session 3.

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
    Converters = { new JsonStringEnumConverter() },      // enums as "Active", not 1
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

> ASP.NET Core does this for you on every request and response, with camelCase by default.
> That's why it's worth knowing what happens under the hood.

### 5.2 XML, briefly

Relevant for the invoicing web service. `XmlSerializer` needs a **public parameterless
constructor and settable properties**, so a positional record won't work here.

```csharp
using System.Xml.Serialization;

[XmlRoot("Invoice")]
public class InvoiceXml
{
    [XmlAttribute("number")] public string Number { get; set; } = "";
    [XmlElement("Total")]    public decimal Total  { get; set; }
}

var serializer = new XmlSerializer(typeof(InvoiceXml));
using var writer = new StringWriter();
serializer.Serialize(writer, new InvoiceXml { Number = "INV-1", Total = 1290m });
```

When a supplier provides an XSD, generate the classes (`xsd.exe` / `dotnet-svcutil`)
instead of writing them by hand.

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

```csharp
public async Task<IReadOnlyList<Order>> LoadOrdersAsync(string path, CancellationToken ct)
{
    await using var stream = File.OpenRead(path);
    var orders = await JsonSerializer.DeserializeAsync<List<Order>>(stream, JsonOptions, ct);
    return orders ?? new List<Order>();
}
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
browser tab during a 30-second report.

```csharp
[HttpGet]
public async Task<ActionResult<IReadOnlyList<ProductListItemDto>>> GetList(
    [FromQuery] string? search, CancellationToken cancellationToken)   // ASP.NET fills it in
{
    var products = await _productService.GetListAsync(null, search, cancellationToken);
    return Ok(products);
}
```

The rule is simple: **accept a `CancellationToken` as the last parameter and pass it to every
async call you make.** The libraries (EF, HttpClient, file APIs) do the actual cancelling.

**In ProductManagement:** follow the token from
[`ProductsController.GetList`](../ProductManagement/src/ProductManagement.Web/Controllers/ProductsController.cs)
→ [`ProductService.GetListAsync`](../ProductManagement/src/ProductManagement.Bll/Services/ProductService.cs)
→ `ToListAsync(cancellationToken)`.

---

## 7. Interfaces and abstraction

### 7.1 Contract vs implementation

An interface says **what** something can do, not **how**.

```csharp
public interface IOrderSource
{
    Task<IReadOnlyList<Order>> LoadAsync(CancellationToken ct = default);
}

internal sealed class JsonFileOrderSource : IOrderSource
{
    private readonly string _path;
    public JsonFileOrderSource(string path) => _path = path;

    public async Task<IReadOnlyList<Order>> LoadAsync(CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(_path);
        return await JsonSerializer.DeserializeAsync<List<Order>>(stream, JsonOptions, ct)
               ?? new List<Order>();
    }
}
```

The code that **uses** orders depends only on `IOrderSource`:

```csharp
public sealed class OrderReport
{
    private readonly IOrderSource _source;
    public OrderReport(IOrderSource source) => _source = source;   // passed in, not created

    public async Task<decimal> TotalAsync(CancellationToken ct = default)
    {
        var orders = await _source.LoadAsync(ct);
        return orders.Sum(o => o.Quantity * o.UnitPrice);
    }
}
```

Tomorrow the orders can come from a database or an HTTP API. `OrderReport` doesn't change.

This connects to session 1: the **interface is `public`**, and the **implementation is
`internal sealed`**. Other projects see the contract, never the details.

### 7.2 Why this makes testing possible

Because `OrderReport` receives an `IOrderSource`, a test can give it a fake one: no
files, no database.

```csharp
public class OrderReportTests
{
    private sealed class FakeOrderSource : IOrderSource
    {
        private readonly IReadOnlyList<Order> _orders;
        public FakeOrderSource(params Order[] orders) => _orders = orders;
        public Task<IReadOnlyList<Order>> LoadAsync(CancellationToken ct = default)
            => Task.FromResult(_orders);
    }

    [Fact]
    public async Task TotalAsync_sums_quantity_times_price()
    {
        // Arrange
        var report = new OrderReport(new FakeOrderSource(
            new Order("A-1", "VEH-001", 2, 100m),
            new Order("A-2", "TYR-001", 3, 10m)));

        // Act
        var total = await report.TotalAsync();

        // Assert
        Assert.Equal(230m, total);
    }
}
```

- xUnit: `[Fact]` = one test, `Assert.*` = the check. Run with `dotnet test`.
- Hand-written fakes are enough to start with. Libraries like **NSubstitute / Moq**
  generate them, so mention them but don't start there.

### 7.3 A preview of DI

"Who creates the `JsonFileOrderSource` and passes it in?" In ASP.NET Core it's the
**dependency injection container**: you register *interface → implementation* once.

```csharp
services.AddScoped<IProductService, ProductService>();
```

**In ProductManagement:** [`IProductService.cs`](../ProductManagement/src/ProductManagement.Bll/Services/IProductService.cs)
(public contract), [`ProductService.cs`](../ProductManagement/src/ProductManagement.Bll/Services/ProductService.cs)
(`internal sealed` implementation),
[`ServiceCollectionExtensions.cs`](../ProductManagement/src/ProductManagement.Bll/Extensions/ServiceCollectionExtensions.cs)
(registration). Lifetimes and the details are covered in session 3.

---

## 8. Lab setup: creating a project in VS Code

Do this once per lab. Every lab is a small console app in its own folder.

**Prerequisites:** .NET SDK (`dotnet --list-sdks`) and the **C# Dev Kit** extension in VS Code.

### 8.1 Create the project

In the VS Code terminal (`` Ctrl+` ``), from the folder where you keep your lab work:

```bash
dotnet new console -n OrdersLab      # creates OrdersLab/ with OrdersLab.csproj + Program.cs
cd OrdersLab
code .                               # open the folder in VS Code (or File → Open Folder)
dotnet run                           # prints "Hello, World!" → the setup works
```

`Program.cs` uses **top-level statements**: there's no `class Program` / `Main`, the file
body *is* `Main`. Keep it short: it only wires things together.

### 8.2 Where does the code go?

One type per file, folders by role. The namespace follows the folder:

```text
OrdersLab/
├── OrdersLab.csproj
├── Program.cs                  ← entry point: read → process → write, nothing else
├── orders.json                 ← input data
├── Models/                     ← records: Order.cs, OrderFailure.cs, ...
├── Sources/                    ← IOrderSource.cs, JsonFileOrderSource.cs, the custom exception
└── Processing/                 ← OrderProcessor.cs: validation + LINQ
```

```csharp
// Models/Order.cs
namespace OrdersLab.Models;          // file-scoped namespace: no extra { } indentation

public sealed record Order(string OrderId, string Sku, int Quantity, decimal UnitPrice);
```

```csharp
// Program.cs
using OrdersLab.Models;              // import the namespaces you use
using OrdersLab.Processing;
```

New file: right-click the folder in the Explorer → **New File…** → `Order.cs`.
(With C# Dev Kit, **New C#…** → *Class / Interface / Record* fills in the namespace for you.)

### 8.3 The input file must be next to the exe

`dotnet run` runs from `bin/Debug/net10.0/`, not from the project folder. Tell the build to
copy the data file there, in the `.csproj`:

```xml
<ItemGroup>
  <None Update="orders.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

…and read it with `Path.Combine(AppContext.BaseDirectory, "orders.json")`.

### 8.4 Run and debug

| What | How |
|---|---|
| Run | `dotnet run` in the terminal |
| Run with arguments | `dotnet run -- offline` (everything after `--` goes to `args`) |
| Debug | `F5` → choose **C#** the first time; breakpoint with `F9`, step with `F10` / `F11` |
| Build errors | **Problems** panel (`Ctrl+Shift+M`) |

### 8.5 Optional: a test project for the stretch goals

```bash
cd ..                                            # next to OrdersLab/, not inside it
dotnet new xunit -n OrdersLab.Tests
dotnet add OrdersLab.Tests reference OrdersLab   # the tests can now see OrdersLab's public types
dotnet new sln -n OrdersLab                      # a solution, so VS Code / Test Explorer sees both
dotnet sln add OrdersLab OrdersLab.Tests
dotnet test
```

Tests only see `public` types. To test an `internal` class, add
`<InternalsVisibleTo Include="OrdersLab.Tests" />` to `OrdersLab.csproj` (session 1).
The **Testing** panel (flask icon) runs and debugs single tests.

---

## 9. Lab 1: JSON in → typed objects → JSON out

Reference solution: [`lecture-2-lab/`](lecture-2-lab/Program.cs) (`dotnet run` in that folder).

**Input**, `orders.json`:

```json
[
  { "orderId": "A-1", "sku": "VEH-001", "quantity": 2, "unitPrice": 1290.00 },
  { "orderId": "A-2", "sku": "TYR-001", "quantity": 4, "unitPrice": 45.50 },
  { "orderId": "A-3", "sku": "",        "quantity": 1, "unitPrice": 10.00 },
  { "orderId": "A-4", "sku": "VEH-001", "quantity": -3, "unitPrice": 1290.00 },
  { "orderId": "A-5", "sku": "BRK-010", "quantity": 1, "unitPrice": 89.90 }
]
```

**Model:**

```csharp
public sealed record Order(string OrderId, string Sku, int Quantity, decimal UnitPrice);
public sealed record OrderFailure(string OrderId, string Reason);
public sealed record SkuTotal(string Sku, int Quantity, decimal Revenue);
public sealed record OrderSummary(IReadOnlyList<SkuTotal> Totals, IReadOnlyList<OrderFailure> Failures);
```

**Steps:**

1. `IOrderSource` + `JsonFileOrderSource`: read the file **asynchronously** with a
   `CancellationToken` (section 6, 7).
2. Validate each order: empty SKU or quantity ≤ 0 goes to the failure list. Don't throw for
   a bad row; it's an expected case (section 4).
3. Summarize the valid orders per SKU with LINQ: `GroupBy` + `Sum`, ordered by revenue
   descending (section 3).
4. Write `summary.json` (camelCase, indented) asynchronously (section 5).
5. A missing input file or malformed JSON is **not** swallowed. Let it surface with a clear
   message (wrap it in a custom exception with the file path).

**Expected output (shape):**

```json
{
  "totals": [
    { "sku": "VEH-001", "quantity": 2, "revenue": 2580.00 },
    { "sku": "TYR-001", "quantity": 4, "revenue": 182.00 },
    { "sku": "BRK-010", "quantity": 1, "revenue": 89.90 }
  ],
  "failures": [
    { "orderId": "A-3", "reason": "SKU is empty" },
    { "orderId": "A-4", "reason": "Quantity must be positive" }
  ]
}
```

**Stretch goals:**

- One xUnit test for the summary logic with a `FakeOrderSource` (section 7.2).
- Also export the totals as XML (section 5.2).
- Break the code on purpose: replace `await` with `.Result`, or remove the `CancellationToken`.
  Discuss what changes.

---

## 10. Lab 2: Price list with live exchange rates

Same shape as lab 1, but the slow part is now the **network**, not a file. This is where
async actually pays off, and where most things can go wrong.
New project as in section 8: `dotnet new console -n PriceLab`, folders `Models/`, `Rates/`,
`Processing/`.

Reference solution: [`lecture-2-lab-2/`](lecture-2-lab-2/Program.cs)
(`dotnet run` for live rates, `dotnet run -- offline` without network).

**Input**, `prices.json` (HUF prices):

```json
[
  { "sku": "VEH-001", "name": "Electric scooter", "priceHuf": 489000 },
  { "sku": "TYR-001", "name": "Tyre 10\"",        "priceHuf": 17990 },
  { "sku": "BRK-010", "name": "Brake pad set",    "priceHuf": 33990 },
  { "sku": "HLM-002", "name": "Helmet",           "priceHuf": 24990 }
]
```

**Rates:** a free ECB-based API, no key needed:

```text
GET https://api.frankfurter.dev/v1/latest?base=HUF&symbols=EUR,USD

{ "amount": 1.0, "base": "HUF", "date": "2026-10-13", "rates": { "EUR": 0.00274, "USD": 0.00307 } }
```

**Model:**

```csharp
public sealed record PriceItem(string Sku, string Name, decimal PriceHuf);
public sealed record ExchangeRates(string Base, string Date, Dictionary<string, decimal> Rates);
public sealed record ConvertedPrice(string Sku, string Name, decimal PriceHuf, decimal PriceEur, decimal PriceUsd);
public sealed record PriceList(string RateDate, IReadOnlyList<ConvertedPrice> Items);

public interface IExchangeRateProvider
{
    Task<ExchangeRates> GetRatesAsync(string baseCurrency, IReadOnlyList<string> targets, CancellationToken ct = default);
}
```

**Steps:**

1. Read `prices.json` asynchronously (as in lab 1).
2. Implement `IExchangeRateProvider` twice (section 7):
   - `FrankfurterRateProvider`: calls the API with `HttpClient.GetFromJsonAsync`;
   - `FixedRateProvider`: a fake with hard-coded rates, no network.
3. Put a **5-second timeout** on the whole run: `new CancellationTokenSource(TimeSpan.FromSeconds(5))`,
   and pass the token everywhere (section 6.5). A timeout is just a cancellation.
4. Convert the prices with LINQ `Select`, looking up the rate in the dictionary. Use
   `decimal` and round explicitly: `Math.Round(x, 2, MidpointRounding.AwayFromZero)`.
5. Write `pricelist.json`.
6. Error handling (section 4). Each failure ends in a clear message and a non-zero exit code,
   never "carry on with a zero rate":

   | Failure | How to try it | Expected handling |
   |---|---|---|
   | API returns 404/500 | `base=XXX` | `HttpRequestException` → wrapped in a custom exception |
   | No network / DNS error | wrong host name | same as above |
   | Response is not the expected JSON | wrong path | `JsonException` → wrapped |
   | Too slow | timeout of `TimeSpan.FromMilliseconds(1)` | `OperationCanceledException`, caught **only at the top** |
   | Currency missing from response | ask only for `EUR` | custom exception, not `KeyNotFoundException` |

**Expected output (shape):**

```json
{
  "rateDate": "2026-10-13",
  "items": [
    { "sku": "BRK-010", "name": "Brake pad set", "priceHuf": 33990, "priceEur": 93.13, "priceUsd": 104.35 }
  ]
}
```

**Stretch goals:**

- xUnit test for the conversion with `FixedRateProvider`: rounding, missing currency.
- Replace one `await` with `.Result` and discuss: it still works in a console app, so why is
  it dangerous in ASP.NET? (section 6.4)
- Export the price list as XML for the invoicing system (section 5.2).

> In ASP.NET Core you don't `new` an `HttpClient`: it comes from `IHttpClientFactory`
> through DI (session 3). That's why the provider gets it through the constructor here.

---

## 11. Homework

A small processor, in the same shape as the lab but on your own input:

- Input: a JSON (or CSV) file, e.g. a price list, a stock movement list, or something
  TL-like.
- Read it into **records**, validate it, produce a summary with LINQ, write a JSON output.
- Requirements:
  - invalid rows end up in a **failure report**, never silently dropped;
  - all file I/O is `async`, with a `CancellationToken` passed through;
  - reading sits behind an **interface**;
  - **at least one xUnit test** using a fake implementation.
- Bring it to session 3. That's where it becomes an HTTP endpoint.

---

## 12. What to take away

- Records for data, `IReadOnlyList<T>` for return values: immutable by default.
- LINQ = extension methods + lambdas. `FirstOrDefault` when it can be missing, `Any` to check existence.
- Never swallow exceptions. Expected outcomes are results or status codes; unexpected ones propagate.
- An error is never a `200 OK` with an info message.
- `System.Text.Json` for JSON; catch `JsonException` at the boundary. XML only where an external system requires it.
- Async is for I/O, not parallelism. `await` all the way up; no `.Result`, no `async void`.
- Accept a `CancellationToken` and pass it down.
- Depend on interfaces; that's what makes code swappable and testable.

**Next session:** HTTP and ASP.NET Core: controllers, DTOs, dependency injection in detail, error middleware with `ProblemDetails`, configuration.
