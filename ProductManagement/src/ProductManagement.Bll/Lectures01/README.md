# Session 1 — .NET platform and C# basics

Lab material for the first session, in the order the topics are taught. The file name
carries the order: `{number}_{name}.cs`.

> The **class** inside each file is `Step01_BuiltInTypes`, `Step02_TypeSystem`, … — a C#
> identifier cannot start with a digit, which is itself worth thirty seconds in step 1.
> All of them share one namespace, `ProductManagement.Bll.Lectures01`, so the types
> defined in one step are visible from the next.

## How each file is built

1. **Supporting types first.** Every file starts with a `TYPES USED BY THE TASKS` block —
   the records and enums the tasks work with, so the group sees the data before the problem.
2. **Task.** A short specification in a comment, then a method whose body is a numbered
   list of `TODO` steps, ending in `throw new NotImplementedException("Task n")`.
3. **Solution.** Directly below, in a `#region Solution n`: the worked solution as real,
   compiling code named `..._Solution`, plus the talking points.

```csharp
public static CleanRow Clean(RawRow raw)
{
    // TODO 1: ...
    // TODO 2: ...
    throw new NotImplementedException("Task 1");
}

#region Solution 1
public static CleanRow Clean_Solution(RawRow raw) { ... }
// talking points
#endregion
```

Project the task, let the group work on the stub, then expand the region and walk through
the solution together. The solutions are runnable — they are not comments.

**Step 1 is different on purpose:** it is a walkthrough rather than a lab. Its sections are
small runnable examples; put the file on the projector and step through them in the
debugger, letting the watch window do the talking. The three tasks at the end are short.

> **Make the regions collapse on open:** *Tools → Options → Text Editor → C# → Advanced →
> "Collapse regions on file open"*. Otherwise press **Ctrl+M, Ctrl+O** before projecting.

## Verifying your work

Every step has a test file in `../../../tests/ProductManagement.Bll.Tests`, one per step:
`Step01_BuiltInTypesTests.cs` … `Step08_ErrorHandlingTests.cs`.

```bash
dotnet test tests/ProductManagement.Bll.Tests
```

The suite is **green out of the box**, because each test file starts with a SWITCH HERE
block pointing at the reference solutions:

```csharp
// --- SWITCH HERE: drop "_Solution" to test your own implementation ---
private static CleanRow Clean(RawRow raw) => Step02_TypeSystem.Clean_Solution(raw);
```

Delete `_Solution` from one line and the tests run against the stub instead - they go red
with `NotImplementedException`, and turn green again as the task is completed. That gives
the group a "make the tests pass" loop without anybody writing a test first.

The tests also pin down the claims the lecture makes: that `0.1 + 0.2 != 0.3` in
`double`, that `Math.Round(2.5m)` is 2, that the broken loop really does return the last
match, and that `throw ex;` really does lose the original call site.

The project still builds: the stubs throw at runtime, they do not break the build. Nothing
here is registered in DI or referenced by the application.

## Agenda

| # | File | Topics | Tasks | Time |
|---|---|---|---|---|
| — | *(no code)* | Intro, goals, the arc of the course | — | 15 min |
| — | [`../../../../dotnet-into.md`](../../../../dotnet-into.md) | The platform from 10 000 feet: runtime, SDK, assembly; PRG files and `STATIC FUNCTION` vs assembly boundaries | — | 30-40 min |
| — | *(the solution itself)* | Project structure and tooling: solution, csproj, NuGet | — | 20 min |
| 1 | `01_BuiltInTypes.cs` | int/long, double vs **decimal**, bool, char, **DateTime / DateTimeOffset / DateOnly / TimeOnly / TimeSpan**, Guid, enum, conversions | 3 | 50-60 min |
| 2 | `02_TypeSystem.cs` | Value vs reference, `var`, nullable, string handling | 3 | 45-55 min |
| 3 | `03_ControlFlow.cs` | `if`/`switch`/switch expressions, patterns, loops, three classic loop bugs | 3 | 40-50 min |
| 4 | `04_Methods.cs` | Default values, named arguments, the Try pattern, parameter objects, overloads | 4 | 40-50 min |
| 5 | `05_ClassesAndVisibility.cs` | `static` vs instance, encapsulation, `private`/`internal`/`public` | 3 | 50-60 min |
| 6 | `06_FileHandling.cs` | `File.ReadAllLines`/`WriteAllText`, `Path.Combine`, encoding, `using`, `StreamReader` | 4 | 30-40 min |
| 7 | `07_Collections.cs` | Array, `List`, `Dictionary`, `HashSet`, comparers, the O(n²) trap | 4 | 40-50 min |
| 8 | `08_ErrorHandling.cs` | `try`/`catch`/`finally`, custom exceptions, throw vs return, four broken catch blocks | 3 | 45-55 min |

That is **5.5 to 6.5 hours** of material, so it is deliberately over-provisioned for a
4-hour session. Pick based on the group:

- **Must keep:** steps 1, 2 and 5. The decimal/date section of step 1 and the
  `internal` part of step 5 are the two highest-value half-hours in the day.
- **Step 8 (error handling) is on the client's must-have list** — if it does not fit,
  move it to the start of session 2 rather than dropping it.
- **Can be shortened:** step 3 (the group already knows loops) and step 6.
- **Can move to homework:** the last task of steps 3, 4, 6 and 7.

## Homework

- Finish any unfinished tasks.
- Bring one example from TL of a procedure with more than six parameters, so we can
  redesign it together at the start of the next session.

## Links back to the real code

Each step ends by opening the equivalent production code in this solution — which is the
point of keeping the lab material in the same repository.

| Step | Open afterwards |
|---|---|
| 1 — built-in types | `../../ProductManagement.Dal/Entities/Product.cs` — `decimal` for money, `DateTime` for `CreatedAtUtc` |
| 5 — classes and visibility | `../Services/ProductService.cs` — `internal` implementation, `public` interface |
| 6 — file handling | `../../ProductManagement.Dal/Seed/SeedData.cs` — the same data, from code instead of a file |
| 7 — collections | `../Services/UserService.cs` — the same operations, against the database |
| 8 — error handling | `../../ProductManagement.Web/Controllers/ProductsController.cs` — `null` from the service becomes a 404 |

## What comes later

LINQ, `System.Text.Json`, `async`/`await` and interfaces/DI are the backbone of the next
session, together with the "must have" list from `desc.md`: unit testing with xUnit,
`ILogger`, `ProblemDetails`, request validation, `CancellationToken`, authentication vs
authorization, secret handling, and the full
Angular → API → Service → data access → DB walkthrough.
