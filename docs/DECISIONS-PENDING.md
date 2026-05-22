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
