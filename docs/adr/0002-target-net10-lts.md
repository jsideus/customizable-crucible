# ADR 0002: Target .NET 10 (LTS)

## Status
Accepted

## Date
2026-05-22

## Context
The framework and the systems it tests must target a specific .NET version. The version choice determines the available runtime libraries, the C# language version, the support lifetime, and — critically for a portfolio piece — the signal it sends to a reviewer about whether the author tracks the platform.

.NET follows a predictable annual release cadence: even-numbered releases are Long Term Support (LTS, three years of support), odd-numbered releases are Standard Term Support (STS, ~18 months). As of this decision:

- .NET 10 is the current LTS, supported until November 10, 2028.
- .NET 8 and .NET 9 both reach end of support on November 10, 2026.

The framework's purpose is to be pointed at modern microservices stacks and to serve as a portfolio artifact for senior engineering roles at engineering-first companies. Both purposes favor the current LTS.

## Decision
Target `net10.0` across all projects in this framework and in the application-under-test it is validated against.

When tooling templates default to an older framework (e.g., the xUnit v3 template floors at `net8.0` for maximum compatibility), explicitly retarget the project's `<TargetFramework>` to `net10.0`.

## Alternatives Considered
- **.NET 8 (previous LTS):** Was the conventional "safe enterprise answer" until late 2025. Rejected because it reaches end of support on the same date as .NET 9 (November 10, 2026), making it no safer than newer versions and signaling that the author is targeting a soon-to-be-unsupported runtime.
- **.NET 9 (STS):** Shorter support window, odd-numbered STS release. Rejected in favor of the LTS for a project intended to remain demonstrable over time.

## Consequences

### Positive
- Three full years of patch and security support (until November 2028).
- Access to C# 14 language features and the latest runtime libraries.
- Signals current-platform fluency to reviewers — "targets current LTS" is the correct modern answer.
- Matches the runtime the framework's test targets will themselves run on, avoiding version mismatch between the framework and the systems under test.

### Negative
- Some tooling templates default to older frameworks and require an explicit retarget step (documented in the relevant runbook).
- Bleeding-edge LTS occasionally surfaces package-compatibility gaps; mitigated by verifying each added dependency builds against net10.0 before relying on it.

## References
- .NET release cadence and support policy: https://dotnet.microsoft.com/platform/support/policy/dotnet-core
