# Runbook 01: Scaffold the xUnit v3 Test Project on Microsoft Testing Platform

**Purpose:** Reconstruct the framework's test-project foundation from nothing — a clean xUnit v3 project on Microsoft Testing Platform, targeting net10.0, registered with an `.slnx` solution.

**Prerequisites:** .NET 10 SDK installed (`dotnet --list-sdks` shows a 10.x entry). Git initialized. Working directory is the repo root (`customizable-crucible/`).

**Decisions this runbook implements:** ADR 0002 (net10), ADR 0003 (xUnit v3), ADR 0004 (Microsoft Testing Platform). Read those for *why*; this runbook is *how*.

---

## Steps

### 1. Create the solution file

```bash
dotnet new sln -n Crucible
```

**Produces `Crucible.slnx`** (the modern XML-based solution format), not `Crucible.sln`, on the .NET 10 SDK.

> **GOTCHA #1 — the template is named `sln`, the output is `.slnx`.**
> The template name did not change when the format modernized. `dotnet new sln` is correct. There is **no** `dotnet new slnx` template — that command errors. On .NET 9/10 SDKs, `dotnet new sln` emits `.slnx` (modern); older SDKs emit `.sln` (legacy). Verify with `ls -la` that you got `Crucible.slnx`.

### 2. Install the xUnit v3 templates (one-time, machine-wide)

```bash
dotnet new install xunit.v3.templates
```

Confirms installation of two templates: `xunit3` (test project) and `xunit3-extension`.

> **GOTCHA #2 — v3 is opt-in; the in-box default is v2.**
> `dotnet new xunit` scaffolds **xUnit v2** (package `xunit`, runs on legacy VSTest). To get **v3** you must first install `xunit.v3.templates`, then use the **`xunit3`** template. If you skip this step and run `dotnet new xunit`, you get the wrong foundation and must tear it out and redo. Always verify the resulting `.csproj` references `xunit.v3.*`, not `xunit`.

### 3. Create the v3 test project

```bash
dotnet new xunit3 -n Crucible.ParallelCore.Tests
```

This scaffolds the project **and** writes/updates `global.json`.

> **GOTCHA #3 — `global.json` gets a test-runner directive.**
> The template writes `{ "test": { "runner": "Microsoft.Testing.Platform" } }` into `global.json`. This tells the .NET 10 SDK to route `dotnet test` through **Microsoft Testing Platform** instead of defaulting to legacy VSTest. Without this directive, `dotnet test` falls back to VSTest and mismatches the MTP-native v3 project. `global.json` governs its directory and all subdirectories — confirm it sits where you intend (repo root governs the whole repo). Verify with `cat global.json`.

### 4. Register the project with the solution

```bash
dotnet sln Crucible.slnx add Crucible.ParallelCore.Tests/Crucible.ParallelCore.Tests.csproj
```

Note the explicit `.slnx` filename — `dotnet sln <file> add` needs the actual filename including extension.

### 5. Retarget from net8.0 to net10.0

Open `Crucible.ParallelCore.Tests/Crucible.ParallelCore.Tests.csproj` and change:

```xml
<TargetFramework>net8.0</TargetFramework>
```
to
```xml
<TargetFramework>net10.0</TargetFramework>
```

> **GOTCHA #4 — the xunit3 template floors at net8.0.**
> The template defaults to `net8.0` because that is xUnit v3's lowest supported framework — it picks the compatibility floor, not your installed SDK version. Per ADR 0002 we target net10.0. Either edit the `<TargetFramework>` line after creation (shown here) or pass the framework option at creation time via `dotnet new xunit3 -?` to discover the flag. Editing the line by hand is fine and worth understanding: `<TargetFramework>` is the single source of truth for the runtime and language version.

### 6. Verify the build against net10.0

```bash
dotnet build Crucible.ParallelCore.Tests/Crucible.ParallelCore.Tests.csproj
```

Expect: `Crucible.ParallelCore.Tests net10.0 succeeded`. If a package conflict surfaces, paste the error and resolve before proceeding — but `xunit.v3` is net10.0-compatible and should build clean.

### 7. Commit the verified foundation

```bash
git add -A
git commit -m "build: scaffold xUnit v3 test project on MTP targeting net10.0"
git push
```

---

## Verification Checklist (the foundation is correct when all are true)

- [ ] `Crucible.slnx` exists at the repo root (modern format).
- [ ] `Crucible.ParallelCore.Tests.csproj` references `xunit.v3.*` (e.g., `xunit.v3.mtp-v2`), **not** `xunit`.
- [ ] The csproj has `<OutputType>Exe</OutputType>` — the standalone-executable marker of a v3/MTP project.
- [ ] The csproj has `<TargetFramework>net10.0</TargetFramework>`.
- [ ] `global.json` contains `"test": { "runner": "Microsoft.Testing.Platform" }`.
- [ ] `dotnet build` succeeds against net10.0.
- [ ] The project is registered in `Crucible.slnx`.

If all seven hold, the foundation is the modern, verified stack: xUnit v3, Microsoft Testing Platform, net10.0, standalone executable.
