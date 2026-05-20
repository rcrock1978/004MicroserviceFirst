---
name: codebase-architecture
description: >
  Use when analyzing the codebase for architectural friction, proposing refactors,
  or deepening module interfaces. Covers deep module design, testability improvements,
  and interface simplification. Trigger on terms like "architecture", "refactor",
  "deep module", "interface design", "testability", "code quality", "module depth",
  "consolidate", "simplify", or "navigability".
---

# Improve Codebase Architecture

Surface architectural friction and propose **deepening opportunities** — refactors that turn shallow modules into deep ones. The aim is testability and AI-navigability.

Adapted from [mattpocock/skills/improve-codebase-architecture](https://github.com/mattpocock/skills).

## Glossary

Use these terms exactly in every suggestion. Consistent language is the point.

- **Module** — anything with an interface and an implementation (function, class, package, slice).
- **Interface** — everything a caller must know to use the module: types, invariants, error modes, ordering, config. Not just the type signature.
- **Implementation** — the code inside.
- **Depth** — leverage at the interface: a lot of behaviour behind a small interface. **Deep** = high leverage. **Shallow** = interface nearly as complex as the implementation.
- **Seam** — where an interface lives; a place behaviour can be altered without editing in place.
- **Adapter** — a concrete thing satisfying an interface at a seam.
- **Leverage** — what callers get from depth.
- **Locality** — what maintainers get from depth: change, bugs, knowledge concentrated in one place.

## Key Principles

1. **Small interfaces, large implementations** — The best modules have simple APIs hiding complex logic.
2. **Information hiding** — Callers should not need to know internal state or ordering constraints.
3. **Error concentration** — Bugs cluster at shallow seams. Deepening moves complexity to testable, local places.
4. **Throwaway adapters** — It should be cheap to swap adapters at a seam for testing or migration.

## Workflow

1. **Explore organically** — Surface shallow modules, tightly-coupled components, and untested seams rather than following rigid heuristics.
2. **Measure depth** — For each module, compare interface complexity to implementation complexity.
3. **Generate alternatives** — Consider multiple radically different interface designs (minimalist, flexible, caller-optimized, ports & adapters).
4. **Recommend the strongest** — Choose the design that maximizes depth and testability.
5. **Document** — Create an RFC documenting the problem space, design trade-offs, and refactoring rationale.

## Anti-patterns to flag

- Interfaces that leak internal state or ordering constraints.
- Modules where the interface is nearly as complex as the implementation.
- Tight coupling that prevents swapping adapters at seams.
- Public setters for business logic state transitions.

## SaaS Platform Alignment

- This skill complements `saas-architecture` by providing a systematic way to evaluate whether service boundaries, handlers, and domain models are deep or shallow.
- Use it when reviewing new services or refactoring existing ones to ensure modules are testable and AI-navigable.
