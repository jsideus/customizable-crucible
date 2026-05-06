# CLAUDE.md - customizable-crucible
## Project Purpose
This repository is a configurable end-to-end test framework targeting **any modern web UI** and **any modern microservices / API stack.** It is built on .NET 10 with C# 14. Its design goal is *portability* - the framework know nothing about any specific AUT / Application Under Test. All AUT-specific configuration is supplied at runtime! It's supplied at runtime by configuration files, environment variables, and dependency injection. 

This is the portfolio piece** for the Jeremy's job search targeting senior SDET / staff QA engineering roles. Code quality here is held to a higher bar than typical project work - every file should be defensible to a staff engineer in a code review.

## Design Principles
- **Portability**: The framework must not have any compile-time knowledge of any specific-AUT. AUT-specific values come from the configuration. The framework is validated against the `chronos-expedition-platform` AUT during development, but the framework repo does NOT reference the AUT repo.
- **Trust through observation**: The framework emits OpenTelemetry traces, metrics, and structured logs for every test execution. Test results aren't pass/fail - they're observable artifacts that can be queried, aggregated, and trended over time.
- **Reliability over speed-without-reliability**: The framework is fast *because* it's reliable, not at the expense of reliability. Flake detection, automatic quarantine, and per-test reliability tracking are first-class features, not afterthoughts.
- **Parallelism with isolation**: All tests run in parallel by default. Test isolation is enforced structurally - no shared mutable state, not inter-test ordering assumptions, deterministic data builders, per-test database schemas or transaction rollback.
- **Configuration over code**: Simply pointing the framework at a new AUT is a configuration exercise, not a code change.

## Tech Stack
- **Language**: C#14
- **Runtime**: .NET 10 (LTS, supported until November 10, 2028)
- **Test runner**: xUnit
- **UI driver**: Playwright for .NET
- **HTTP client**: Refit (for typed REST clients) and `HttpClient` for ad-hoc calls
- **GraphQL client**: StrawberryShake or direct `HttpClient` (decision pending - see ADRs)
- **Mocking / stubs**: WireMock.Net for stand-in services
- **Container orchestration**: Testcontainers for .NET (Postgres, RabbitMQ, etc.)
- **Contract testing**: PactNet (planned, post-foundations)
- **Reporting**: Allure Framework (.NET adapter)
- **Telemetry**: OpenTelemetry SDK, OTLP exporter, configurable backend (Honeycomb / Datadog / local Jaeger)

## Repository Structure

customizable-crucible/
├── src/
│   ├── Crucible.Core/             # Configuration, DI, base abstractions
│   ├── Crucible.Web/              # UI testing primitives (Playwright wrappers, page objects base)
│   ├── Crucible.Api/              # REST/GraphQL testing primitives (Refit setup, response assertions)
│   ├── Crucible.Data/             # DB assertion helpers (Dapper-based)
│   ├── Crucible.Messaging/        # Message-bus assertion helpers
│   ├── Crucible.Reliability/      # Flake detection, quarantine, retry policies
│   └── Crucible.Telemetry/        # OpenTelemetry instrumentation
├── tests/
│   └── Crucible.SelfTests/        # The framework testing itself
├── examples/
│   └── ChronosExpeditions/        # Example test suite targeting the chronos AUT
├── docs/
│   ├── adr/                       # Architecture Decision Records
│   └── usage/                     # How-to guides for configuring against new AUTs
└── .github/workflows/             # CI pipelines, including running tests against chronos AUT

## Conventions and Standards

- Modern C# idioms: File-scoped name spaces, primary constructors, nullable reference types enabled, records for value-like types, target-typed `new`, `required` members
- Public API surface is documented with XML doc comments because this is a *framework* - consumers will hover over symbols and expect documentation
- All public types have unit tests
- All public types have at least one integration test demonstrating realistic usage
- Async all the way down - no `.Result` or `.Wait()`, ever!
- Cancellation tokens on every async public method
- Structured logging with `ILogger<T>`; framework also emits open telemetry events for each significant operation
- Test Naming: `MethodName_Scenario_ExpectedBehavior` or BDD-style `Given-When-Then` - pick one per project and stay consistent

## What NOT to do

- **No reflection-based magic** without a documented justification. Frameworks that just "work" via reflection are nightmares to debug for users. Prefer explicit configuration.
- **No required runtime dependency on cloud services**. Why? The framework must run fully offline against local Testcontainers. Cloud is optional, never mandatory.
- **No assumptions about the AUT's tech stack** inside core libraries. AUT-specific assumptions belong in adapter libraries or in the consuming test suite, not in `Crucible.Core` or `Crucible.Web`.
- **No silent failures ever.** A test that doesn't run because of a misconfiguration *must* fail LOUDLY! A clear error message, not pass-by-default or skip-quietly. 
- **No "AI suggested this" code without justification.** Every single implementation choice is articulate and defensible.

## Current focus

[Will update this section as the work progresses]

**Active phase**: Foundations practice (2 week estimate). Jeremy is rebuilding from-scratch coding skills using canonical resources before beginning the capstone implementation. The capstone "slice" work begins after foundations completes.

## Working with Me (Notes for Claude Code)
- The repo owner (Jeremy) is **deliberately rebuilding from-scratch coding skills**. Default to *not* writing code. Default to accurately teaching, explaining, asking Socratic questions, and reviewing code the repo owner (Jeremy) has written.
- This repo is held to a much higher overall quality bar than the chronos-expedition-platform AUT repo. Code reviews of this repository should be at the level a staff engineer at Datadog or Honeycomb.io would apply to a production framework or codebase.
- When generating code is genuinely warranted, keep it minimal and explain every single non-obvious line.
- The owner (Jeremy) is preparing for rigorous technical interviews. Code that ships here may very likely be discussed and defended inside interview loops. Optimize for the owner's ability to articulate every design decision.
- Architectural decisions are recorded as ADRs in `docs/adr/`. Significant changes should reference or update and ADR. 