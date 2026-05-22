# ADR 0005: Per-Test Isolation Strategy

## Status
Accepted

## Date
2026-05-22

## Context
This is the bedrock decision of the framework. Every other capability — parallel execution, scale, flake-free results, trustworthy output — rests on it.

The framework guarantees **thread-safe, per-test parallel execution at any scale.** Delivering that guarantee requires solving one problem precisely: when many tests run concurrently, they must not interfere with one another. Test interference is the dominant source of flake — the intermittent, non-deterministic failures that destroy an organization's trust in a test suite. If developers cannot trust the results, the suite has negative value: it costs time and confidence without preventing defects.

### The root cause of test interference: the danger trifecta
Concurrent test interference requires three conditions to hold simultaneously:

1. **Shared** — two or more tests can reach the same state (a static field, a singleton, a database row, a file, a cache entry).
2. **Mutable** — that shared state can be changed.
3. **Concurrent** — the tests access it at overlapping times.

Remove any one leg and interference becomes impossible:
- Remove **shared** → each test has its own copy; nothing to contend over.
- Remove **mutable** → the state is immutable; concurrent reads are always safe.
- Remove **concurrent** → sequential access; no overlap.

A critical clarification that drives the entire design: `static` and `singleton` are **not** immutable. `static` means *one shared copy at the class level, living for the application's lifetime* — fully mutable unless also marked `readonly`/`const`. A singleton is *one shared instance for the app's lifetime*, typically holding mutable state. These constructs supply the "shared + mutable" two-thirds of the trifecta; parallelism supplies the "concurrent." Together they produce flake.

### The economic constraint
Removing the "concurrent" leg is off the table — parallelism is the headline feature, not a thing to sacrifice. Removing the "mutable" leg universally is impractical — tests need to create, modify, and verify state. That leaves removing the **shared** leg as the primary strategy: give every test its own isolated state.

But isolation has a cost, and a naive reading ("each test gets its own everything") suggests an expensive, poorly-scaling solution. The engineering case must be matched by an equally strong **business case**, or the framework is indefensible to a cost-conscious organization. The key insight that reconciles them: *per-test isolation means isolated **state**, not isolated **environments**.* The cost of isolating state varies by orders of magnitude depending on which layer is isolated, and the correct strategy is to isolate at the **cheapest layer that provides a sufficient guarantee.**

## Decision
**Remove the "shared" leg of the danger trifecta by giving every test its own isolated state, using the cheapest isolation mechanism that provides a sufficient guarantee for the layer under test.**

Concretely, follow the principle: **share the expensive infrastructure; isolate only the cheap state.**

### The isolation cost spectrum (cheapest to most expensive)

| Layer | Mechanism | Cost | When to use |
|---|---|---|---|
| In-process | Fresh object construction per test (own in-memory store, own service instances, no statics, no shared singletons) | Microseconds — just allocations + later GC | Default for unit/component tests; the starting point |
| Database (shared engine) | Transaction-rollback teardown, or per-test schema/key namespacing, against one shared database engine | Milliseconds | Integration tests needing real persistence |
| Database (fresh instance) | Fresh container per test (e.g., Testcontainers) | Seconds | Only when transaction/namespace isolation is insufficient |
| Full environment | Separate deployed stack per test | Very expensive | Almost never; reserved for true end-to-end isolation requirements |

The framework defaults to the cheapest sufficient layer and escalates only when a specific test genuinely requires stronger isolation. xUnit's fixture model (ADR 0003) is the enabling mechanism: `IClassFixture`/`ICollectionFixture` share the *expensive* resource (the database engine, the message broker) across a scope, while the per-test new-instance default isolates the *cheap* state (data, connections, transactions).

## Why This Scales — the Business Case
Because expensive infrastructure is shared and only cheap state is duplicated, **isolation cost scales sub-linearly with test count.** Adding the ten-thousandth in-process test costs microseconds of additional isolation overhead, not a new environment. Adding the ten-thousandth database test costs a transaction, not a new database. The framework therefore delivers flake-free parallelism at a cost an organization can afford at scale — the engineering guarantee (no interference) and the economic guarantee (sub-linear cost) hold simultaneously.

Stated for a cost-conscious stakeholder: *"Per-test isolation does not mean per-test environments — that would be prohibitively expensive. It means isolated state, achieved at the cheapest layer that provides a sufficient guarantee. We share the expensive infrastructure and duplicate only the cheap state, so flake-free parallel execution scales sub-linearly with test count. Expensive isolation is reserved only for the rare cases that genuinely require it."*

## Alternatives Considered
- **Rely on test-author discipline to avoid shared state:** Rejected. Discipline is not a guarantee; it fails silently and intermittently, which is precisely the flake the framework exists to eliminate. Isolation must be structural, not aspirational.
- **Isolate everything at the most expensive layer (environment per test) for maximum safety:** Rejected. Provides no additional guarantee over cheaper layers for most tests while destroying the economic case. Over-isolation is as much an engineering failure as under-isolation.
- **Eliminate parallelism to remove the "concurrent" leg:** Rejected. Parallelism is the headline capability; sacrificing it defeats the framework's purpose.
- **Make all state immutable:** Rejected as a universal strategy. Tests inherently create and mutate state to verify behavior; universal immutability is impractical at the test layer.

## Consequences

### Positive
- Flake from shared-state interference is eliminated structurally, independent of test-author discipline.
- Isolation cost scales sub-linearly — the framework is economically viable at large scale.
- The strategy is layer-appropriate: cheap isolation where cheap suffices, expensive isolation only where required.
- Directly enables the trust-in-results goal: deterministic, non-interfering tests produce results the organization can rely on for go/no-go decisions before production.

### Negative
- Requires per-layer judgment about which isolation mechanism is "sufficient" — a design burden carried by the framework rather than pushed to test authors (this is intentional; it is where the framework's value concentrates).
- Shared expensive infrastructure (database engine, broker) becomes a point requiring its own careful lifecycle management via fixtures; correctness of the fixture-scoping is load-bearing.

## References
- Meszaros, *xUnit Test Patterns* (2007) — "Database Sandbox," "Transaction Rollback Teardown," and the isolation-strategy cost catalog.
- ADR 0003 (xUnit v3) — the fixture model that operationalizes "share expensive, isolate cheap."
