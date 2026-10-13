# The .NET platform from 10,000 feet

> Session 1 / block 2 — background material.
> Audience: experienced Harbour developers for whom .NET is new.
> The goal is not completeness, but a correct **mental model** of what gets
> compiled, what actually runs, and where the boundaries in the code are drawn.

---

## 1. What is ".NET"?

".NET" is an umbrella term covering three distinct things — and most early
confusion comes from blurring them together:

| Layer | What it is | Harbour parallel |
|---|---|---|
| **Runtime** (CLR) | The thing that *executes* compiled code: JIT compiler, garbage collector (GC), type loader | the Harbour runtime library linked into the EXE |
| **BCL** | The built-in class library: files, collections, HTTP, JSON, dates… | the Harbour RTL plus the shared function library grown over the years |
| **SDK** | The developer toolchain: compiler (Roslyn), MSBuild, the `dotnet` CLI, templates, NuGet client | the Harbour compiler + linker + make |

Practical consequences:

- A **developer machine needs the SDK** (which includes the runtime).
- A **server only needs the runtime** — or not even that, if you publish
  *self-contained*, because then the runtime ships inside the output.

How to check:

```bash
dotnet --info          # shows everything: SDKs, runtimes, OS, RID
dotnet --list-sdks
dotnet --list-runtimes
```

### .NET Framework vs .NET (Core)

What you will run into in old code and old StackOverflow answers:

- **.NET Framework 1.0–4.8.1** — the old platform: *Windows only*, shipped as part
  of Windows, installed in exactly one version per machine. No longer under
  functional development, security fixes only.
- **.NET Core 1.0–3.1 → .NET 5, 6, 7, 8, 9, 10…** — the rewritten, cross-platform,
  open-source platform that can be **installed side by side in several versions**.
  The "Core" was dropped at .NET 5, and version 4 was skipped so it would not clash
  with .NET Framework 4.x.

**Only the second branch is relevant for rewriting TL.** If you see
".NET Framework" somewhere, it is almost certainly outdated information.

---

## 2. The compilation chain: what happens when you run `dotnet build`?

```
  .cs sources            IL (Intermediate Language)         machine code
       │                            │                             │
       │   Roslyn (C# compiler)     │      JIT (at runtime)        │
       ├───────────────────────────►├────────────────────────────►│
       │                            │                             │
  MyApp/*.cs                  MyApp.dll                     x64 instructions
                          (= one assembly)                     in memory
```

There are two stages, and this is the key difference from Harbour:

1. **At build time** the C# sources do not become machine code. They become **IL** —
   a platform-independent intermediate language — plus **metadata** (which types,
   methods and fields exist, and with what visibility). Both go into one `.dll`.
2. **At runtime** the JIT (Just-In-Time) compiler compiles the methods that are
   *actually called* into native code for that machine. What is never called is
   never compiled.

A few consequences that feel unusual at first:

- A `.dll` is **not machine code**, and it is **not only code** — it is
  self-describing: the full set of types can be read back from the metadata at any
  time. That is why IntelliSense works on a NuGet package, why reflection exists,
  and why decompilation is possible (ILSpy, dotPeek).
- There is no classic **link step** fusing everything into one EXE. Referenced
  assemblies stay separate files and are loaded at runtime.
- For a .NET application the `.exe` is just a thin launcher; the real code sits in
  the `.dll` of the same name next to it.

> **One clarification worth making early:** C# has `private`, `internal` and
> `public`, but these are **compile-time** rules recorded in metadata. Reflection
> can bypass them. They are not a security boundary — they are a **design tool**.

### Two optional side notes (not needed on day one, but good to know they exist)

- **ReadyToRun (R2R)**: precompiled native code is included at publish time →
  faster startup, larger files.
- **Native AOT**: a fully native binary with no JIT. Very fast startup, small
  memory footprint, at the cost of limited reflection. Worth it for console tools
  and background services; usually not for a classic web backend.

---

## 3. What is an assembly?

The **assembly** is the most important unit of organisation in .NET. Physically it
is a `.dll` (or `.exe`) file. Logically it is **four** things at once:

1. **Unit of deployment** — this is what you copy, what the runtime loads, and what
   goes into a NuGet package.
2. **Unit of versioning** — the version number belongs to the assembly, not to a
   class or a method.
3. **Boundary of type identity** — a type's real identity is `Namespace.Type`
   **plus the assembly** it lives in. If the same class exists in two assemblies,
   the runtime sees **two different types**, and assigning one to the other fails
   with `InvalidCastException`. (A classic, hard-to-find source of bugs.)
4. **Boundary of visibility** — and for what follows, this is the interesting one.

One project (`.csproj`) compiles into **exactly one** assembly. In other words:
**project ≈ assembly**. So when we decide how many projects to split TL into, what
we are really designing is where the visibility and versioning boundaries go.

### Namespace ≠ assembly

Worth separating these up front, because they often coincide but are not the same:

- A **namespace** is only **name organisation**, so that identically named classes
  do not clash. It has no effect on visibility whatsoever.
- An **assembly** is a real **boundary** — it is exactly what `internal` means.

One assembly can contain many namespaces, and one namespace can span several
assemblies.

---

## 4. The parallel: PRG files and C-style visibility vs assembly boundaries

This is the point of the whole block.

### What you know from Harbour / C

| Harbour / C | What it means |
|---|---|
| a `.prg` file | one compilation unit (translation unit) |
| `STATIC FUNCTION` / C `static` | callable **only from that file** |
| `FUNCTION`, `PROCEDURE` / C `extern` | callable **from the entire linked program** |
| file-level `STATIC` variable | global state scoped to one file |
| the linker | fuses everything into a single EXE; a name clash is a link-time error |

So in practice there are **two** levels: *file-scoped* and *everyone sees it*.
There is nothing in between. The moment you want to call a function from a second
PRG, it becomes reachable from the entire program — including all the code that has
nothing to do with it.

In a system with several thousand procedures this means it is very hard to say who
gets hurt when you change a function. Anyone can call it, so you have to assume
everyone does.

### What C# gives you instead

C# has **five** levels, and the middle one is exactly what Harbour is missing:

| Modifier | Visible from | Roughly equivalent to |
|---|---|---|
| `private` | the declaring class only | *no equivalent* (narrower than a single PRG) |
| `protected` | the class and its derived classes | *no equivalent* |
| `internal` | **the whole assembly, but not outside it** | `STATIC FUNCTION`, but scoped to a whole module |
| `protected internal` | assembly + derived classes | rare |
| `public` | anyone referencing the assembly | `FUNCTION` / `extern` |

**The key sentence:** `internal` is like `STATIC FUNCTION`, except it is scoped to
an entire **project** rather than to a single *file*.

This is precisely the level missing from TL today. In a `ProductManagement.Dal`
project every SQL-adjacent helper class can be `internal`: freely usable inside the
project, while **the compiler refuses** to let `Bll` touch it. Not a convention,
not something a code review has to catch — it simply does not compile.

```csharp
// ProductManagement.Dal assembly
internal sealed class SqlParameterBuilder { }   // only from inside Dal
public  sealed class ProductRepository   { }    // visible from outside too
```

If `Bll` tries to use `SqlParameterBuilder`:

```
error CS0122: 'SqlParameterBuilder' is inaccessible due to its protection level
```

### Two practical additions

- **Defaults.** With no modifier, a *class* defaults to `internal` and a *member*
  defaults to `private`. This is deliberate: by default everything is as narrow as
  possible. Harbour is the opposite — everything is global by default — so this is
  a reflex worth consciously retraining.
- **`InternalsVisibleTo`.** An assembly can grant one *specific* other assembly
  access to its `internal` members — typically its own unit test project:

  ```xml
  <ItemGroup>
    <InternalsVisibleTo Include="ProductManagement.Dal.Tests" />
  </ItemGroup>
  ```

### What to take away

| Question | Harbour | .NET |
|---|---|---|
| What is the compilation unit? | the `.prg` file | the **project** (= assembly) |
| How many visibility levels? | 2 | 5 |
| What protects the internals? | convention, naming | the **compiler** |
| What runs on the server? | one large EXE | an app + assemblies + runtime |
| When do name clashes surface? | at link time | at compile time, avoided by namespaces |

The practical lesson for the rewrite: designing the module boundaries is not
paperwork, it is **the first real architectural decision**. Whatever you make
`public` is a contract that is hard to walk back; whatever you leave `internal` you
can rewrite at will.

---

## 5. Lifecycle of .NET versions

### The cadence

Since 2020 the release schedule has been predictable, and it is worth knowing by
heart:

- A new major version ships **every November**.
- **Even** major version = **LTS** (Long Term Support) → **3 years** of support.
- **Odd** major version = **STS** (Standard Term Support) → **18 months**.
- Monthly security and bugfix patches in between (e.g. 6.0.36).
- Multiple major versions install **side by side** on the same machine — there is
  no need for one big jump; you can decide per application.

### Summary table

*(State as of 2026-10-12. Authoritative, always-current source:
https://dotnet.microsoft.com/platform/support/policy/dotnet-core)*

| Version | Released | Type | End of support | Status |
|---|---|---|---|---|
| .NET Core 3.1 | 2019-12-03 | LTS | 2022-12-13 | out of support |
| .NET 5 | 2020-11-10 | STS | 2022-05-10 | out of support |
| **.NET 6** | 2021-11-08 | LTS | **2024-11-12** | **out of support (~2 years)** |
| .NET 7 | 2022-11-08 | STS | 2024-05-14 | out of support |
| **.NET 8** | 2023-11-14 | LTS | **2026-11-10** | **expiring (~1 month)** |
| .NET 9 | 2024-11-12 | STS | 2026-05-12 | out of support |
| **.NET 10** | 2025-11-11 | **LTS** | **2028-11-14** | **current LTS** |
| .NET 11 | 2026-11 (expected) | STS | 2028-05 (expected) | not released yet |

> "Out of support" means no more security fixes. It does not mean it stops running —
> it means a newly discovered vulnerability will not be patched.

### What does this mean for us concretely?

Per `desc.md`, which version to focus on is still an open question. From an
engineering standpoint the picture is fairly clear:

- **.NET 6** — most of the current codebase lives here, but it has been **out of
  support for two years**. Fine to learn on, not worth building new code on.
- **.NET 8** — goes out of support within a month. Acceptable as a waypoint, not as
  a destination.
- **.NET 10** — the current LTS, supported until late 2028. **This is the realistic
  target for a rewrite starting now.**

**Suggestion for the curriculum:** the language and architecture core of the
material is version-independent — what is true on .NET 6 about DI, async/await or
layering is equally true on .NET 10. So it is worth:

1. running the **practice project on .NET 10** (LTS), because that is what the
   rewrite is aiming at;
2. adding a short separate block on **what changed between 6 and 10** for the
   existing .NET 6 code (minimal hosting API, language features, the evolution of
   `System.Text.Json`);
3. treating the upgrade itself as a topic — in practice it is changing
   `<TargetFramework>`, updating NuGet packages, and then working through the
   breaking changes.

If the client still picks .NET 6 as the focus, that is fine too: in the demo
solution the `global.json` + `Directory.Build.props` pair means the target version
is changed in **one place**.

### What actually determines what runs?

Three concepts get mixed up regularly, even though they are set independently:

- **`<TargetFramework>` (e.g. `net6.0`)** — what we *compile against*: which APIs
  are available and which C# language version is the default.
- **SDK version (`global.json`)** — what we *compile with*. A .NET 10 SDK can
  happily compile for `net6.0`; running the result still requires a 6.0 runtime.
- **Roll-forward** — if the targeted runtime is not installed, the host by default
  only moves up to the nearest higher *minor* version; it will not cross a major
  version on its own. That is why a `net6.0` app does not start on a machine that
  only has .NET 10 installed.

---

## 6. Mini glossary: Harbour → .NET

| Harbour | .NET / C# | Note |
|---|---|---|
| `.prg` file | `.cs` file | but the boundary is the project, not the file |
| compiled `.obj` + linker | `.dll` (assembly) | no fusing; they stay separate files |
| `STATIC FUNCTION` | `internal` | scoped to a **project**, not a file |
| `FUNCTION` / `PROCEDURE` | `public` method | but in C# it lives on a class, not on its own |
| global function library | NuGet package / project reference | versioned, explicitly referenced |
| `#include` | `using` + project/package reference | `using` is only a namespace shorthand |
| make / build script | `.csproj` + MSBuild | the project file *is* the build script |
| Harbour runtime inside the EXE | .NET runtime on the machine (or self-contained) | |

---

## 7. What to watch out for in week one

- Do not look for the "main PRG": in an ASP.NET Core application `Program.cs` is the
  entry point, but it is not a control loop — it is **assembly work**. It registers
  services and then hands control to the web server.
- Splitting into projects is not cosmetic. If everything lives in one project,
  `internal` protects nothing and you are back to the Harbour "everyone sees
  everything" situation.
- `bin` and `obj` are generated output — they never go into version control.
- If an error message says `could not load file or assembly`, it is almost always a
  versioning or deployment problem, not a compilation one.

---

## Related material

- Demo solution: [`ProductManagement/`](ProductManagement/README.md) — layering and
  `internal` boundaries in practice.
