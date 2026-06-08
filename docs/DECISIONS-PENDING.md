# Pending Decisions

A living log of decisions deliberately deferred. Each entry records what is undecided, why it was deferred, and the slice or milestone at which it should be resolved. Deferrals here are intentional — this file exists so they are not silently forgotten and do not harden into accidental commitments.

When a pending decision is resolved, move it out of this file and into an ADR.

---

## P-01: Test result data model
**Status:** Deferred to Slice 2.
**Decision needed:** The structured form in which test results are captured — test id, outcome, duration, thread/worker id, trace id, seed, captured inputs, failure detail. Everything downstream (observability, flake tracking, quarantine, RCA reporting, dashboards) reads from this model.
**Why deferred:** Slice 1 is scoped to proving parallel execution only. The data model is the Slice 2 centerpiece.
**Why it matters:** This is the expensive-to-retrofit decision. If bolted on late, every downstream feature must be refactored. Design it as early as Slice 2 so features layer on cleanly.

## P-02: BDD layer (Reqnroll) — include or cut
**Status:** Undecided.
**Decision needed:** Whether to include a Gherkin/BDD layer (Reqnroll) at all.
**Why deferred:** BDD adds value only when non-engineer stakeholders read the specs, and is pure overhead otherwise. The audience for this framework's specs is not yet defined.
**Guidance:** For a portfolio piece demonstrating engineering capability, a deliberate decision to *cut* BDD — with the reasoning articulated — is a stronger signal than reflexive inclusion. Resolve by answering: who reads the test specs?

## P-03: Scope of the "any microservices stack" claim
**Status:** Undecided — needs honest scoping before it hardens.
**Decision needed:** The precise, defensible boundary of what the framework targets.
**Why it matters:** "Points at ANY microservices stack" is an indefensible universal claim (gRPC? GraphQL subscriptions? SSE? Kafka? mixed auth?). A scoped claim — "configurable across REST and GraphQL HTTP APIs over common auth schemes, with a documented extension model for other protocols" — is stronger in an interview than an overpromise. Decide the boundary and the extension point.

## P-04: Determinism toolkit
**Status:** Future design concern.
**Decision needed:** Design of the framework's determinism primitives beyond state isolation — injectable clock (kill `DateTime.Now` flake), seeded randomness, explicit ordering (never depend on unsorted collection order), controlled async (retries, timeouts, race handling).
**Why it matters:** Flake elimination is broader than shared-state isolation (ADR 0005 handles only the state-interference source). The framework claims flake elimination as a headline feature, so it must address all flake sources. Design as a first-class "determinism toolkit" in a later slice.

## P-05: Page Object / Screenplay pattern for the UI layer
**Status:** Future design concern (lands with Playwright integration).
**Decision needed:** How to encapsulate UI structure to keep selectors stable and out of test bodies — Page Objects vs. the Screenplay pattern.
**Why it matters:** Connects to selector stability as a UI-layer flake source. Decide when Playwright is integrated.

## P-06: Contract testing vs. integration testing boundary
**Status:** Future design concern.
**Decision needed:** Where to use consumer-driven contract testing (Pact) versus full integration/E2E. Contract tests catch a different bug class (API shape drift breaking a consumer) far more cheaply and with less flake than full E2E, and should be the preferred way to verify service boundaries.
**Why it matters:** Refines the framework's opinion about what to test where — part of test-pyramid discipline. Resolve as the API-testing layers mature.
 
 ## P-07: Runner and driver swappability -- extent, timing, and the framework's structural commitment
 **Status:** Future design concern
 **Decision needed:** Three layers stay strictly tooling-agnostic: result schema (Crucible.Core), reporters (Crucible.Reporting), and Page Objects / domain operations (Crucible.PageObjects). Two layers are tooling-specific by design and isolated as adapters: runner adapters (Crucible.Driver.Playwright, Crucible.Driver.Selenium). Test methods have exactly one tooling-coupled element: the discovery attribute (`[Fact], [Test]`). Test bodies call into Page Objects and assertions that themselves are tooling-agnostic. 
 **Architecture:**  Design `Crucible-core` to be strictly runner agnostic from the start (no `using xUnit` or xUnit types leak into Core source). Build the first runner adapter (`Crucible.Runner.Xunit`) as a separate project when result-collection lifecycle hooks become necessary. Defer formalizing an `ITestResultCollector` interface until a second concrete adapter (TUnit, MSTest, NUnit) is implemented and the interface can be designed from observed commonality rather than speculation. The Hexagonal/Ports-andAdapters patter (Cockburn 2005; same architecture as EF Core's provider model) is the target shape.
 **Value proposition:** institutional test logic investment is preserved across tool churn.
 **Trade off:** BDD frameworks (Reqnroll) bind step definitions to feature files in a framework-specific way, making them more tightly coupled than plain attribute-driven tests -- to be addressed in P-02.
 **Cost:** one extra project per supported runner, modest interface-design discipline at the seam.
 **Benefit:** framework outlives runner churn, demonstrates architectural maturity for portfolio evaluation.

## P-08: HTTP-client seam for Crucible.Api — to become ADR 0006
**Status:** Gated. Must be decided before the second Crucible.Api test lands.
**Decision needed:** How `HttpClient` is constructed and reused per test (`IHttpClientFactory` + DI vs per-test `new HttpClient()` vs xUnit fixture-scoped client), and where the base URL is sourced (env var, `IConfiguration`, fixture parameter, test constant). Slice 1's single test hardcodes the URL inline; that one-liner is the deliberate placeholder this decision replaces.
**Why deferred:** Slice 1 is one test against a stateless hardcoded GET — no seam pressure yet, and a premature abstraction here would shape itself by speculation rather than observed need.
**Why it matters:** Intersects ADR 0005. `HttpClient` is shared *expensive* infrastructure (sockets, DNS, handler pooling) — its state must be either immutable across tests or rebuilt per test, or the framework reintroduces the exact shared-mutable-state risk 0005 designs out. The seam is also load-bearing for downstream concerns: auth-token caching, correlation-ID propagation (`DelegatingHandler`), retry/Polly policies, and per-environment URL injection.