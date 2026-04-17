<!--
Sync Impact Report
- Version change: unversioned template → 1.0.0
- Modified principles:
  - Template Principle 1 → I. Code Quality Is Non-Negotiable
  - Template Principle 2 → II. Testing Standards Define Done
  - Template Principle 3 → III. User Experience Must Be Consistent
  - Template Principle 4 → IV. Performance Requirements Are Explicit
  - Template Principle 5 → V. Spec-to-Code Traceability Is Required
- Added sections:
  - Engineering Standards
  - Delivery Workflow & Quality Gates
- Removed sections:
  - None
- Templates requiring updates:
  - ✅ .specify/templates/plan-template.md
  - ✅ .specify/templates/spec-template.md
  - ✅ .specify/templates/tasks-template.md
  - ⚠ pending .specify/templates/commands/*.md (directory not present in repository)
  - ✅ README.md
- Deferred TODOs:
  - TODO(RATIFICATION_DATE): original adoption date is not recorded in repository history.
-->
# Tarea-clase-SDD Constitution

## Core Principles

### I. Code Quality Is Non-Negotiable
All production code MUST be readable, maintainable, and reviewable. Every pull
request MUST include clear naming, single-responsibility functions/classes, and
removal of dead code or commented-out logic. Reviewers MUST reject changes with
unexplained duplication, ambiguous naming, or avoidable complexity.  
Rationale: Sustainable delivery requires code that future contributors can
understand and modify safely.

### II. Testing Standards Define Done
Features are not complete until automated tests validate expected behavior.
Changes MUST include tests derived from specification scenarios and MUST cover
happy paths, validation failures, and critical edge cases. Bug fixes MUST add a
regression test that fails before the fix and passes after it.  
Rationale: Test-backed changes reduce regressions and keep implementation aligned
with the agreed specification.

### III. User Experience Must Be Consistent
API and application behavior MUST be consistent across similar user actions.
Equivalent operations MUST use consistent validation rules, response structure,
error semantics, and domain terminology. Any intentional UX deviation MUST be
documented in the specification and reviewed before implementation.  
Rationale: Consistency lowers user error rates and improves predictability.

### IV. Performance Requirements Are Explicit
Each feature specification MUST define measurable performance targets (for
example response-time, throughput, and/or resource limits) relevant to the
feature scope. Implementations MUST include objective verification steps (tests,
benchmarks, or profiling evidence) when performance constraints are declared.  
Rationale: Explicit performance expectations prevent accidental degradation and
late-stage rework.

### V. Spec-to-Code Traceability Is Required
Implementation, tests, and tasks MUST trace back to documented scenarios and
requirements. Every delivered feature MUST show a verifiable path from spec
scenarios to code changes and test evidence. If code behavior changes, the spec
MUST be updated in the same workstream.  
Rationale: Traceability is the foundation of Spec-Driven Development and
ensures shared understanding between planning and delivery.

## Engineering Standards

- Language and framework conventions MUST be enforced via formatter/linter or
  documented team standards.
- Pull requests SHOULD remain focused and small enough for complete review in a
  single session; large changes MUST include a decomposition rationale.
- Public contracts (DTOs, API endpoints, request/response models) MUST include
  validation rules and failure behavior.
- Team members MUST not merge changes with unresolved critical review comments.

## Delivery Workflow & Quality Gates

1. Specification-first: A feature spec with Given/When/Then scenarios MUST exist
   before implementation starts.
2. Planning alignment: The implementation plan MUST pass a constitution check
   for quality, testing, UX consistency, and performance criteria.
3. Task traceability: Tasks MUST be organized by user story and include explicit
   test and validation work.
4. Verification gate: Before merge, contributors MUST show passing automated
   tests plus evidence that acceptance scenarios are satisfied.
5. Review gate: At least one reviewer MUST confirm constitution compliance in
   the pull request.

## Governance

This constitution supersedes conflicting local process notes for engineering
delivery decisions. Amendments MUST be proposed through a documented pull
request that includes: (a) proposed text changes, (b) migration/rollout impact,
and (c) version bump rationale under semantic versioning.

Versioning policy for this constitution:
- MAJOR: incompatible governance changes, principle removals, or principle
  redefinitions that alter mandatory behavior.
- MINOR: new principles/sections or materially expanded requirements.
- PATCH: clarifications, wording improvements, and non-semantic edits.

Compliance reviews MUST occur in planning and pull-request phases. The
implementation plan's Constitution Check and final PR review checklist MUST
explicitly confirm adherence to all principles.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): original adoption date unknown | **Last Amended**: 2026-04-17
