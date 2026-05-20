---
name: tdd
description: >
  Use when building features or fixing bugs using test-driven development.
  Covers red-green-refactor loops, vertical slicing, behavior-focused integration tests,
  and testability design. Trigger on terms like "TDD", "test-first", "red-green-refactor",
  "write tests", "testing", "behavior test", "integration test", or "vertical slice".
---

# Test-Driven Development

## Philosophy

**Core principle**: Tests should verify behavior through public interfaces, not implementation details. Code can change entirely; tests shouldn't.

**Good tests** are integration-style: they exercise real code paths through public APIs. They describe *what* the system does, not *how* it does it. A good test reads like a specification — "user can checkout with valid cart" tells you exactly what capability exists. These tests survive refactors because they don't care about internal structure.

**Bad tests** are coupled to implementation. They mock internal collaborators, test private methods, or verify through external means (like querying a database directly instead of using the interface). The warning sign: your test breaks when you refactor, but behavior hasn't changed. If you rename an internal function and tests fail, those tests were testing implementation, not behavior.

Adapted from [mattpocock/skills/tdd](https://github.com/mattpocock/skills).

## Anti-Pattern: Horizontal Slices

**DO NOT write all tests first, then all implementation.** This is "horizontal slicing" — treating RED as "write all tests" and GREEN as "write all code."

This produces **brittle tests**:
- Tests are written against imagined interfaces that change during implementation.
- Tests miss edge cases because they weren't informed by real code.
- Tests become coupled to the first implementation rather than behavior.

## Vertical Slicing (The Correct Way)

**RED** — Write one failing test for one behavior.
**GREEN** — Write the simplest code to make that test pass.
**REFACTOR** — Clean up duplication and deepen modules while keeping the test green.

Repeat. Each cycle produces one behavior + one test + clean code.

## Planning Phase

Before writing any test, confirm:
1. What interface change is needed?
2. What behaviors should be tested (priority order)?
3. How will the code be designed for testability?

## Refactoring Guidelines

Refactor only after all tests pass:
- Extract duplication.
- Deepen modules (small interface, large implementation).
- Apply SOLID principles.
- Ensure tests remain green throughout.

## Mocking Guidelines

- Mock at seams (external dependencies, not internal collaborators).
- Prefer real implementations for in-memory dependencies.
- Mock only what crosses a process or network boundary.

## SaaS Platform Alignment

- This skill complements `saas-testing`. Use TDD for unit and application layer tests.
- For integration tests with Testcontainers, plan the test first, then spin up containers, then verify behavior through public APIs.
- The Result monad makes behavior testing easier: assert on `Result.IsSuccess` or `Result.IsFailure` rather than catching exceptions.
