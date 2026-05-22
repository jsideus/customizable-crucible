# ADR 0003: xUnit v3 as the Test Runner

## Status
Accepted

## Date
2026-05-22

## Context
The framework needs a test runner — the engine that discovers, schedules, executes, and reports tests. This choice is foundational because the framework's headline guarantee is **thread-safe per-test parallel execution at scale**, and the runner's default execution and isolation model either works with that guarantee or against it.

The .NET test-runner field has three established options: xUnit, NUnit, and MSTest. Each also has a major-version dimension: xUnit shipped a ground-up v3 rewrite (stable as of 2025, built natively on Microsoft Testing Platform), distinct from the long-deployed v2.

The deciding criterion is not popularity but **alignment of the runner's default isolation model with the framework's core thesis.** A framework selling per-test isolation should be built on a runner whose defaults enforce isolation, so the framework works with the grain rather than fighting the runner's defaults.

## Decision
Use **xUnit v3** as the test runner.

## Alternatives Considered

- **NUnit:** More flexible and feature-rich, but its default is to share a single test-class instance across all tests in that class. For a framework whose entire premise is per-test isolation, this default works *against* the thesis — every test author would have to actively counteract the shared-instance default. Rejected: wrong default for an isolation-first framework.

- **MSTest:** Microsoft's original framework, now also runs on Microsoft Testing Platform and has improved substantially. Historically the weakest isolation model and the least greenfield momentum. Rejected: no advantage over xUnit for this use case and weaker isolation heritage.

- **xUnit v2:** The widely-deployed stable default (what `dotnet new xunit` scaffolds). Runs on the legacy VSTest engine. Rejected in favor of v3 because v3 is the actively-invested-in architecture (see ADR 0004 on Microsoft Testing Platform) and represents the modern execution model. Choosing v2 would mean building a new portfolio framework on the older engine.

## Why xUnit's Defaults Fit
xUnit constructs a **new instance of the test class for every test method**, with no shared mutable state unless the author explicitly opts into sharing via fixtures (`IClassFixture<T>` for per-class shared resources, `ICollectionFixture<T>` for cross-class shared resources). Lifecycle is expressed through the constructor (setup) and `IDisposable`/`IAsyncDisposable` (teardown), which maps exactly to the per-test-isolation / shared-expensive-infrastructure pattern this framework is built on (see ADR 0005).

This isolation-by-default posture means the framework removes the "shared state" leg of the thread-safety danger trifecta structurally, by virtue of the runner's design, rather than by relying on test-author discipline.

## Consequences

### Positive
- Runner defaults enforce the isolation the framework guarantees — working with the grain.
- Fixture model provides a clean, explicit mechanism for the "share expensive infrastructure, isolate cheap state" cost strategy (ADR 0005).
- v3 is built natively on Microsoft Testing Platform (ADR 0004), the modern execution architecture.
- Collection-level parallelism configuration gives precise control over parallelism granularity.

### Negative
- The test runner is a smaller, more swappable architectural commitment than the browser-automation layer (Playwright) will be; this decision matters but is lower-stakes and more reversible than the driver-layer choices.
- v3 requires explicit template installation (`xunit.v3.templates`) and is not the in-box default; documented in the scaffold runbook.

## References
- xUnit v3 documentation: https://xunit.net/docs/getting-started/v3/getting-started
- Meszaros, *xUnit Test Patterns* (2007) — fixture and isolation pattern catalog.
