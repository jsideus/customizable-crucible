# ADR 0004: Microsoft Testing Platform over Legacy VSTest

## Status
Accepted

## Date
2026-05-22

## Context
Beneath the test framework (xUnit, ADR 0003) sits the *execution engine* — the lower layer that actually discovers and runs the compiled tests and reports results to `dotnet test`, IDEs, and CI. Two engines exist:

- **VSTest:** The engine that has driven `dotnet test`, Visual Studio Test Explorer, and their predecessors since Visual Studio 2010. Mature, ubiquitous, architecturally dated.
- **Microsoft Testing Platform (MTP):** A ground-up replacement created by the same team, designed to be leaner, faster-starting, more deterministic, and far more extensible. xUnit v3 is built natively on MTP.

This decision is directly analogous to a transition the test-automation field already lived through at the browser layer: Selenium (HTTP/WebDriver protocol, architecturally older) losing its position to Playwright (persistent WebSocket connection, auto-waiting, more deterministic). The lesson transfers — when a newer engine wins on architecture and is where the platform owner is investing, building new work on the older engine is choosing legacy.

## Decision
Run the framework's tests on **Microsoft Testing Platform**, not legacy VSTest.

Concretely:
- The xUnit v3 package referenced is the MTP-native variant (`xunit.v3.mtp-v2`).
- The project's `<OutputType>` is `Exe` — under MTP, test projects compile to **standalone executables** rather than DLLs loaded by an external runner.
- `global.json` carries the directive `"test": { "runner": "Microsoft.Testing.Platform" }`, which instructs the .NET 10 SDK to route `dotnet test` through MTP instead of defaulting to VSTest.

## Alternatives Considered
- **Legacy VSTest:** The default `dotnet test` engine on the .NET 10 SDK absent the global.json directive. Rejected because it is the architecturally older engine, not where platform investment is going, and mismatched with xUnit v3's MTP-native design. Building a new portfolio framework on the legacy engine would be the Selenium choice.

## Why the Architecture Matters
Under MTP, each test project is a self-contained executable that can be run directly (`dotnet run`, or invoking the built binary). This eliminates the external-runner indirection of VSTest, where test DLLs are loaded and driven by a separate host process. The standalone-executable model is faster to start, more deterministic, and more extensible — properties that compound at scale, which is exactly the regime this framework targets (thread-safe parallel execution of large test suites in CI).

For a framework whose value proposition includes *speed* and *determinism* as preconditions for *trust in results*, the execution engine's determinism and startup characteristics are not incidental — they are part of the product.

## Consequences

### Positive
- Faster, more deterministic execution — preconditions for the framework's "results the whole org trusts" goal.
- Standalone-executable model simplifies CI invocation and reduces external-runner variability.
- NuGet-based extensibility model (MTP) supports the framework's future observability, flake-tracking, and reporting extensions cleanly.
- Aligns with the platform owner's active investment direction.

### Negative
- MTP is newer; some third-party tooling and IDE integrations matured around VSTest first. Mitigated by xUnit v3's native MTP support and the maturity reached by 2026.
- Requires the global.json directive on .NET 10 SDK; an undocumented or missing directive silently falls back to VSTest. Captured explicitly in the scaffold runbook.

## References
- Microsoft Testing Platform (xUnit v3 docs): https://xunit.net/docs/getting-started/v3/microsoft-testing-platform
- "What's New in xUnit v3" — standalone executables and MTP rationale: https://xunit.net/docs/getting-started/v3/whats-new
