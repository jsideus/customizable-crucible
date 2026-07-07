# Playwright .NET Runtime Architecture

## Purpose

This document captures the runtime architecture behind the `customizable-crucible` test framework by combining runtime observation (distributed tracing and observability) (e.g. EventPipe for runtime) with source analysis (source code and reasoning) to reach architectural conclusions. It helps answer the questions "what does the source code explicitly do?", "what actually happens when we execute `dotnet test`?", and "what framework rule can Crucible derive from the combination of source code and runtime evidence?".

The goal is to understand and document how Playwright .NET, xUnit v3, Microsoft Testing Platform, and .NET cooperate to support isolated, thread-safe, parallel browser-based end-to-end tests.

## Observability Stack
Level 1
========

dotnet-trace

↓

Observe CLR

----------------------------

Level 2

ParallelProofLogger

↓

Observe framework decisions

----------------------------

Level 3

OpenTelemetry

↓

Observe distributed traces

----------------------------

Level 4

GitHub Actions artifacts

↓

Persist evidence

----------------------------

Level 5

Honeycomb / Jaeger / Aspire Dashboard

↓

Visualize entire test suite

## Technology Stack Under Review

- .NET 10
- Microsoft Testing Platform
- xUnit v3
- `xunit.v3.mtp-v2`
- `Microsoft.Playwright.Xunit.v3`
- Playwright .NET
- GitHub Actions

## Core Architecture Principle

Playwright is the browser automation engine.

xUnit v3 is the test scheduler.

Microsoft Testing Platform is the execution platform.

GitHub Actions is the CI/CD orchestrator.

Crucible is the framework policy layer responsible for preserving isolation, observability, artifact strategy, test metadata, and enterprise maintainability.

## Non-Negotiable Test Contract

Every test must be able to run:

- alone
- repeatedly
- in any order
- in parallel
- after failure
- in CI/CD
- without depending on another test

## Runtime Chain Under Review

CloudDevPlatformTests
    ↓
PageTest
    ↓
ContextTest
    ↓
BrowserTest
    ↓
PlaywrightTest
    ↓
WorkerAwareTest
    ↓
IAsyncLifetime

**Review Questions Applied to Every Layer**

For each runtime layer, this document answers:
* Ownership
* Creation
* Disposal
* Lifetime
* Shared State
* Concurrency Guarantees
* Thread Safety
* Failure Modes
* Framework Implications

# Chapter 1 - Process Lifetime

## Chapter Goal

Understand the process-level runtime root for Playwright .NET when executed through:

```text
dotnet test
    ↓
Microsoft Testing Platform
    ↓
xUnit v3
    ↓
Microsoft.Playwright.Xunit.v3
```

The objective of this chapter is to determine how the Playwright runtime is created, owned, shared, and managed before introducing any framework abstractions.

---

## Investigation Methodology

Every architectural decision in this document must be traceable to evidence.

No architectural conclusion should be accepted based on assumption, intuition, or opinion.

Every investigation follows the same workflow:

```text
Question
    ↓
Source Analysis
    ↓
Runtime Analysis
    ↓
Verified Findings
    ↓
Architectural Decision
```

Runtime Analysis may consist of one or more evidence sources:

* Framework Instrumentation
* Runtime Diagnostics (EventPipe / dotnet-trace)
* Runtime Observations

---

## Runtime Layer Under Investigation

```text
dotnet test
    ↓
Microsoft Testing Platform
    ↓
xUnit v3
    ↓
PlaywrightTest
    ↓
IPlaywright
```

---

## Classes Under Review

* PlaywrightTest
* Playwright
* IPlaywright

---

## Open Questions

1. When is `_playwrightTask` created?
2. What thread creates `_playwrightTask`?
3. Is initialization thread-safe?
4. Can multiple `IPlaywright` instances exist within the same test process?
5. Who owns the `IPlaywright` instance?
6. Who disposes the `IPlaywright` instance?
7. What happens if `Playwright.CreateAsync()` fails?
8. What framework rules should Crucible derive from this lifetime?

---

# Investigation 001

## Question

**When is `_playwrightTask` created?**

---

## Source Analysis

### Source Observation

`PlaywrightTest` inherits from `WorkerAwareTest`.

```csharp
public class PlaywrightTest : WorkerAwareTest
```

`PlaywrightTest` defines a static process-level task responsible for creating the Playwright runtime.

```csharp
private static readonly Task<IPlaywright> _playwrightTask =
    Microsoft.Playwright.Playwright.CreateAsync();
```

Every test instance later awaits the same task during initialization.

```csharp
public override async ValueTask InitializeAsync()
{
    await base.InitializeAsync().ConfigureAwait(false);

    Playwright = await _playwrightTask.ConfigureAwait(false);

    BrowserName = PlaywrightSettingsProvider.BrowserName;
    BrowserType = Playwright[BrowserName];

    Playwright.Selectors.SetTestIdAttribute("data-testid");
}
```

---

### Source Interpretation

The implementation defines a single static `Task<IPlaywright>` field.

From the source code we can conclude:

* `_playwrightTask` belongs to the `PlaywrightTest` type.
* `_playwrightTask` does **not** belong to an individual test instance.
* Individual tests do **not** call `Playwright.CreateAsync()`.
* Every test instance awaits the same `_playwrightTask`.

---

### Evidence Classification

**Status:** Source Verified

Evidence:

* `PlaywrightTest` defines a static readonly `Task<IPlaywright>`.
* `InitializeAsync()` awaits `_playwrightTask` instead of creating a new Playwright instance.

---

## Runtime Analysis

### Runtime Instrumentation

To observe runtime behavior, two temporary probe tests were created.

Each probe logged:

* `RuntimeHelpers.GetHashCode(Playwright)`
* `Environment.ProcessId`
* `Environment.CurrentManagedThreadId`

---

### Observed Runtime Output

```text
2026-06-25T23:32:54.9920380+00:00 | PID=34510 | Thread=6 | PLAYWRIGHT_PROBE PlaywrightRuntimeProbeTwo PlaywrightHash=16116045 PID=34510 Thread=6

2026-06-25T23:32:54.9920670+00:00 | PID=34510 | Thread=7 | PLAYWRIGHT_PROBE PlaywrightRuntimeProbeOne PlaywrightHash=16116045 PID=34510 Thread=7
```

---

### Runtime Interpretation

Runtime observation confirmed:

* Both probe tests executed within the same .NET process.
* Both probe tests received the same `IPlaywright` instance.
* Both probe tests executed on different managed threads.

---

### Runtime Diagnostics

**Current Status:** Planned

The following diagnostics will be captured in a future investigation using EventPipe:

* `dotnet-trace`
* `dotnet-stack`
* `dotnet-counters`

Planned observations:

* CLR type initialization timing
* Static field initialization thread
* Async execution flow
* Runtime scheduling behavior

---

### Evidence Classification

**Status:** Runtime Verified

---

## Verified Findings

### Creation

`_playwrightTask` is created by the static field initializer defined within `PlaywrightTest`.

**Status:** Verified

---

### Ownership

`PlaywrightTest` owns the process-level access point to the shared `Task<IPlaywright>`.

Individual test classes do not create `IPlaywright` directly.

**Status:** Verified

---

### Lifetime

Runtime observation confirmed that multiple test classes reused the same `IPlaywright` instance within the observed UI test process.

**Status:** Verified

---

### Shared State

The shared process-level object is `IPlaywright`.

Browser instances, browser contexts, and pages are not part of this investigation.

**Status:** Verified

---

### Disposal

Unknown.

Further investigation required.

---

### Thread Safety

Partially Verified.

Further investigation required.

---

### Concurrency Guarantees

Partially Verified.

Further investigation required.

---

### Failure Modes

Unknown.

Further investigation required.

---

## Architectural Decision

### Rule 001

Crucible shall **not** instantiate `Playwright.CreateAsync()`.

Crucible shall inherit from `PageTest` and allow `Microsoft.Playwright.Xunit.v3` to own the lifetime of the process-level `IPlaywright` instance.

Test-specific state shall never be stored on `IPlaywright`.

---

## Remaining Unknowns

The following questions remain unanswered:

* Which thread executes the static field initializer?
* Who ultimately disposes `IPlaywright`?
* What happens if `Playwright.CreateAsync()` throws?
* Can a faulted `_playwrightTask` recover?
* What CLR guarantees govern static field initialization?

These questions will be answered through subsequent investigations.

---

# Chapter Summary

## Questions Answered

✓ When is `_playwrightTask` created?

---

## Constraints Established

* `PlaywrightTest` defines a single static `Task<IPlaywright>`.
* Multiple test classes reused the same `IPlaywright` instance within the observed UI test process.
* Individual test classes do not instantiate Playwright.

---

## Architectural Decisions

* Crucible shall not instantiate `Playwright.CreateAsync()`.
* Crucible shall rely on `Microsoft.Playwright.Xunit.v3` to own process-level Playwright initialization.

---

## Next Investigation

**Investigation 002**

**Question:**

> What thread creates `_playwrightTask`?


























# Chapter 1 - Process Lifetime

Source Evidence

↓

Runtime Probe

↓

Trace Evidence

↓

Conclusion

## Chapter 1 Methodology

Each runtime conclusion must be supported by three evidence types:

1. Source Analysis  
   What the source code explicitly defines.

2. Runtime Observation  
   What is observed during `dotnet test` execution using runtime probes or .NET diagnostics tooling.

3. Architectural Conclusion  
   What Crucible is allowed to assume or must enforce.

No claim should be promoted to a verified finding unless it is supported by source evidence, runtime evidence, or both.

## Goal
**Understand the process-level runtime root for Playwright .NET when executed through:

dotnet test
    ↓
Microsoft Testing Platform
    ↓
xUnit v3
    ↓
Microsoft.Playwright.Xunit.v3

## Runtime Layer

dotnet test
    ↓
Microsoft Testing Platform
    ↓
xUnit v3
    ↓
PlaywrightTest
    ↓
IPlaywright

## Classes Under Review
**PlaywrightTest**
**Playwright**
**IPlaywright**

## Source Code Under Review

**C#**: private static readonly Task<IPlaywright> _playwrightTask = Playwright.CreateAsync();

## Open Questions
1. When is _playwrightTask created?
2. What thread creates it?
3. Is initialization thread-safe?
4. Can multiple `IPlaywright` instances be created in the same test process?
5. Who owns the `IPlaywright` instance?
6. who disposes of the `IPlaywright` instance?
7. What failure modes exist if `Playwright.CreateAsync()` fails?
8. What framework rules should Crucible derive from this lifetime?

## Source Observation 001 - `PlaywrightTest` Defines the process-level Playwright root

**The `PlaywrightTest` class inherits from the `WorkerAwareTest` class.**

```csharp
public class PlaywrightTest : WorkerAwareTest
```
**Inside `PlaywrightTest`, Playwright is initialized through a static readonly task:**

```csharp
private static readonly Task<IPlaywright> _playwrightTask =
    Microsoft.Playwright.Playwright.CreateAsync();
```
**Each test instance later awaits this task during initialization:**

```csharp
public override async ValueTask InitializeAsync()
{
    await base.InitializeAsync().ConfigureAwait(false);

    Playwright = await _playwrightTask.ConfigureAwait(false);
    BrowserName = PlaywrightSettingsProvider.BrowserName;
    BrowserType = Playwright[BrowserName];
    Playwright.Selectors.SetTestIdAttribute("data-testid");
}
```
## Immediate Architectural Interpretation
`_playwrightTask` is static, so it belongs to the `PlaywrightTest` type, not to an individual instance. 
That means `Playwright.CreateAsync()` is intended to run once per loaded `PlaywrightTest` type within the
test process, and __each__ test instance awaits the __same__ task.

This source code proves that `PlaywrightTest` defines `_playwrightTask` as a static field. This means that for each .NET process/AppDomain, there is only one `_playwrightTask` field per loaded `PlaywrightTest` type. Whether the resulting
`IPlaywrightTest` instance is reused by all tests in this process must, scratch that, **will** be verified by runtime observation using EventPipe. EventPipe is a .NET runtime-level diagnostics 

## Evidence Classification

Status: Source-Derived

Evidence: 
   - `PlaywrightTest` contains a static readonly `Task<IPlaywright>`. 
   - InitializeAsync() awaits the shared task instead of calling `Playwright.CreateAsync()` per test.


## Runtime Observation 001:

**During execution of two independent** `PageTest` subclasses within the same UI test process, both test classes received an `IPlaywright` instance with identical object identity 
(`RuntimeHelpers.GetHashCode`) while executing on different managed threads.

# Ownership

`PlaywrightTest` owns the process-level access point to `IPlaywright` through the static `_playwrightTask` field.
That means that the individual test classes do not create `IPlaywright` directly.

# Creation

`IPlaywright` creation is instantiated by the static field initializer:

```csharp
private static readonly IPlaywright _playwrightTask = 
    Microsoft.Playwright.Playwright.CreateAsync();
```

This creation is associated with the `PlaywrightTest` type, not with the test class `CloudDevPlatformTestsOne`, or `PageTest` class, or `ContextTest` class, or an individual [Fact] method.

# Disposal

Evidence required:
- Inspect whether `PlaywrightTest` overrides `DisposeAsync()`
- Inspect whether `IPlaywright` is disposed **directly**.
- Inspect whether disposal is delegated to worker services.

# Lifetime

The `_playwrightTask` field is static, so its lifetime is tied to the loaded `PlaywrightTest` type inside the test process.

Practical Implication: the `IPlaywright` instance is effectively process-scoped for the test process. 

# Shared State

The `_playwrightTask` is shared across all test instances that inherit from `PlaywrightTest`.

The shared object is the Playwright runtime entry point, not a browser context, page, or test data object.

# Thread Safety

Evidence required:
- Confirm the Common Language Runtime static field initialization semantics.
- Confirm whether multiple test instances await the same `_playwrightTask`.
- Verify runtime behavior with parallel probe tests.


# Concurrency Guarantees

Evidence required:
- Run multiple test classes in parallel
- Log `RuntimeHelpers.GetHashCode(Playwright)`. 
- Confirm same process and same `IPlaywright` reference.

# Failure Modes

Evidence required:
- Determine behavior if `Playwright.CreateAsync()` fails.
- Determine whether `_playwrightTask` can recover or remain faulted.

# Framework Implications

Crucible should **not** create its own IPlaywright instance per test.

Crucible should build above `PageTest` and allow `Microsoft.Playwright.Xunit.v3` to own Playwright runtime initialization. 

Crucible framework code should treat `IPlaywright` as infrastructure-level state and should not store test specific data on it.

## Chapter 2 - Worker Lifetime

Source Evidence

↓

Runtime Probe

↓

Trace Evidence

↓

Conclusion

# Goal

**Understand how `WorkerAwareTest` manages work-scoped services.**

# Runtime Layer
WorkerAwareTest
    ↓
Worker
    ↓
Worker.Services

# Classes under review
- `WorkerAwareTest`
- `Worker`
- `IWorkerService`

# Open Questions
1. When is a worker created?
2. When is a worker reused?
3. What data is stored inside a worker?
4. Can two tests use the same worker at the same time?
5. What services are worker-scoped?
6. How does worker lifetime affect browser lifetime?

# Ownership
TBD

# Creation
TBD

# Disposal
TBD

# Lifetime
TBD

# Shared State
TBD

# Thread Safety
TBD

# Concurrency Guarantees
TBD

# Failure Modes
TBD

# Framework Implications
TBD

## Chapter 3 - Browser Lifetime

Source Evidence

↓

Runtime Probe

↓

Trace Evidence

↓

Conclusion

# Goal

- Understand how browser instances are created, cached, reused, and disposed.

# Runtime layer
BrowserTest
    ↓
BrowserService
    ↓
IBrowser

# Classes Under Review

`BrowserTest`
`BrowserService`
`IBrowser`

# Open Questions
1. Is the browser created per test, per worker, or per process?
2. How does `BrowserService.Register(...)` cache browser instances?
3. Who disposes the browser?
4. Can browser instances be sharded safely?
5. How does browser lifetime affect parallel execution?

# Ownership
TBD

# Creation
TBD

# Disposal
TBD

# Lifetime
TBD

# Shared State
TBD

# Thread Safety
TBD

# Concurrency Guarantees
TBD

# Failure Modes
TBD

# Framework Implications
TBD

## Chapter 4 - BrowserContext Lifetime

Source Evidence

↓

Runtime Probe

↓

Trace Evidence

↓

Conclusion

# Goal
**Understand test-level browser isolation.**

**Runtime Layer**
ContextTest
    ↓
IBrowserContext

# Classes Under Review
- ContextTest
- IBrowserContext
- BrowserNewContextOptions

# Open Questions
1. Is a new IBrowserContext created for every test?
2. What state does a browser context isolate?
3. Who owns the context?
4. Who disposes the context?
5. Can a context be reused safely?
6. What framework rules should prevent context leakage?

# Ownership
TBD

# Creation
TBD

# Disposal
TBD

# Lifetime
TBD

# Shared State
TBD

# Thread Safety
TBD

# Concurrency Guarantees
TBD

# Failure Modes
TBD

# Framework Implications
TBD

## Chapter 5 - Page Lifetime

# Goal

**Understand page creation and ownership per test.**

# Runtime Layer
PageTest
    ↓
IPage

# Classes Under Review
- PageTest
- IPage

# Open Questions
1. Is a new IPage created for every test?
2. Who owns the page?
3. Who disposes the page?
4. Can a page be shared safely?
5. How should page objects relate to IPage?

# Ownership
TBD

# Creation
TBD

# Disposal
TBD

# Lifetime
TBD

# Shared State
TBD

# Thread Safety
TBD

# Concurrency Guarantees
TBD

# Failure Modes
TBD

# Framework Implications
TBD

## Chapter 6 - Test Class Lifetime

Source Evidence

↓

Runtime Probe

↓

Trace Evidence

↓

Conclusion

# Goal

**Understand how xUnit v3 creates and schedules test classes.**

# Runtime Layer
xUnit v3
    ↓
Test Class
    ↓
Fact Method

# Classes Under Review
- CloudDevPlatformTestsOne
- CloudDevPlatformTestsTwo
- FactAttribute
- ITestOutputHelper

# Open Questions

1. Does xUnit create a new test class instance per test?
2. How does xUnit schedule test classes?
3. What is the default test collection behavior?
4. How does class-level parallelism work?
5. What kinds of shared context disable or restrict parallelism?

# Ownership
TBD

# Creation
TBD

# Disposal
TBD

# Lifetime
TBD

# Shared State
TBD

# Thread Safety
TBD

# Concurrency Guarantees
TBD

# Failure Modes
TBD

# Framework Implications
TBD

## Chapter 7 - Verified Findings

Source Evidence

↓

Runtime Probe

↓

Trace Evidence

↓

Conclusion

# Finding 001 - Microsoft Testing Platform Is The Active Runner

- The project uses Microsoft Testing Platform, not the legacy VSTest command surface.
Evidence:

dotnet test --logger "console;verbosity=detailed"

failed with:

Unknown option '--logger'

This indicates that legacy VSTest logger options are not valid in the current execution path.

# Finding 002 - xUnit v3 Uses Different Output Namespaces Than xUnit v2

- Xunit.Abstractions is not available in this project.

`ITestOutputHelper` is available from:

using Xunit;

This confirms that xUnit v2 examples should not be blindly copied into this xUnit v3/MTP stack.

# Finding 003 - Class-Level Parallel Execution Was Observed

- Two UI test classes were observed running concurrently in the same process.

**Evidence included:**

1. Same process ID
2. Multiple managed thread IDs
3. Interleaved test execution
4. Corrupted unsynchronized file writes when both tests appended to the same file concurrently

**Conclusion:**
The current stack can execute separate test classes concurrently.

# Finding 004 - Unsynchronized Shared File Writes Are Not Thread-Safe

- The first parallel proof logger used unsynchronized File.AppendAllText.

**Observed result:**

partial/corrupted log lines

**Conclusion:**
Shared resources introduced by Crucible must be synchronized or avoided entirely.

## Chapter 8 - Framework Design Rules

Source Evidence

↓

Runtime Probe

↓

Trace Evidence

↓

Conclusion

# Rule 001 - No Static Mutable Test State

Static mutable test state can be shared across parallel tests and can introduce race conditions.

# Rule 002 - No Shared IPage

Each test must use its own IPage.

# Rule 003 - No Shared IBrowserContext

Each test must use its own IBrowserContext.

# Rule 004 - Page Objects Are Test-Scoped

Page objects must wrap the current test's IPage and must not be static or singleton.

# Rule 005 - Test Data Must Be Isolated

Each test must create or reference data that is unique, immutable, or safely isolated.

# Rule 006 - Shared Files Require Synchronization

Any shared file write must use a lock, mutex, or isolated per-test file path.

# Rule 007 - Tests Must Not Depend On Execution Order

Every test must be executable independently.

# Chapter 9 - CI/CD Implications

Source Evidence

↓

Runtime Probe

↓

Trace Evidence

↓

Conclusion

**Goal**

Translate the runtime lifetime model into GitHub Actions execution rules.

**Open Questions**

1. How should test assemblies be executed in GitHub Actions?
2. How should test reports be collected?
3. How should Playwright traces be stored?
4. How should artifacts be named?
5. How should parallel execution be configured?
6. How should failure diagnostics be preserved?

# Framework Implications
TBD