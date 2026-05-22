# Documentation Conventions

This directory holds the project's durable engineering knowledge. It is organized into three categories, each serving a distinct purpose and a distinct reader.

## Structure

```
docs/
├── README.md              # This file — the convention itself
├── adr/                   # Architecture Decision Records — WHY the system is built this way
│   ├── 0001-record-architecture-decisions.md
│   ├── 0002-target-net10-lts.md
│   ├── 0003-test-runner-xunit-v3.md
│   ├── 0004-microsoft-testing-platform-over-vstest.md
│   └── 0005-per-test-isolation-strategy.md
├── runbook/               # Reproducible step-by-step procedures — HOW to rebuild or operate
│   └── 01-test-project-scaffold.md
└── DECISIONS-PENDING.md   # Open decisions not yet made — what's deferred and why
```

## The Three Categories

**ADRs (`adr/`)** answer *why*. Each records one architectural or design decision: the context that forced it, the decision made, the alternatives rejected, and the consequences accepted. ADRs are immutable once Accepted — a superseding decision is a new ADR that references the old one. They are the interview-defense artifacts and the institutional memory of the project's reasoning. Format follows Michael Nygard's convention (see ADR 0001).

**Runbooks (`runbook/`)** answer *how*. Each is a numbered, copy-pasteable procedure for rebuilding or operating part of the system, with the reasoning behind each step and any non-obvious gotchas captured inline. A runbook is the panic-recovery artifact: if all memory of the build were erased, the runbooks alone would let it be reconstructed exactly.

**Pending decisions (`DECISIONS-PENDING.md`)** answer *what's not decided yet*. A single living log of deferred decisions, each with the reasoning for deferral and the slice or milestone at which it should be resolved. It exists so that deliberate deferrals are not silently forgotten and do not harden into accidental commitments.

## Naming

- ADRs: `NNNN-kebab-case-title.md`, numbered sequentially, never renumbered.
- Runbooks: `NN-kebab-case-title.md`, numbered in build order.
- One file per ADR. One file per runbook. One pending-decisions log for the whole project.

## Principle

Minimum beautiful product: three directories, clear names, one index. Capture every non-trivial decision and every reproducible procedure, but add no ceremony beyond what serves discovery and reconstruction. The test of this documentation is simple — could a competent engineer, with memory erased, rebuild the system and defend every decision in it using only these files? If yes, the documentation is complete. If no, a stone is unturned.
