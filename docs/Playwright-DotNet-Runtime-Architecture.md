# Playwright .NET Runtime Architecture

## Purpose

This document captures the runtime architecture behind the `customizable-crucible` test framework.

The goal is to understand and document how Playwright .NET, xUnit v3, Microsoft Testing Platform, and .NET cooperate to support isolated, thread-safe, parallel browser-based end-to-end tests.

Crucible is not merely a Playwright test suite. It is intended to become an enterprise-grade browser automation framework where every test can run independently, repeatedly, in any order, and in parallel inside CI/CD.

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

## Chapter 1 - Process Lifetime

# Goal
**Understand the process-level runtime root for Playwright .NET when executed through:

dotnet test
    ↓
Microsoft Testing Platform
    ↓
xUnit v3
    ↓
Microsoft.Playwright.Xunit.v3

# Runtime Layer

dotnet test
    ↓
Microsoft Testing Platform
    ↓
xUnit v3
    ↓
PlaywrightTest
    ↓
IPlaywright

# Classes Under Review
**PlaywrightTest**
**Playwright**
**IPlaywright**

# Source Code Under Review

**C#**: private static readonly Task<IPlaywright> _playwrightTask = Playwright.CreateAsync();

# Open Questions
1. When is _playwrightTask created?
2. What thread creates it?
3. Is initialization thread-safe?
4. Can multiple `IPlaywright` instances be created in the same test process?
5. Who owns the `IPlaywright` instance?
6. who disposes of the `IPlaywright` instance?
7. What failure modes exist if `Playwright.CreateAsync()` fails?
8. What framework rules should Crucible derive from this lifetime?

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

## Chapter 2 - Worker Lifetime

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