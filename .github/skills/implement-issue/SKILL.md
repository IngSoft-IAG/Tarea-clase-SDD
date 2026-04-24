---
name: implement-issue
description: Fetch a GitHub issue by number and implement it following the existing codebase patterns. Writes production code only — no tests. Use when the user wants to implement a specific issue.
---

# Implement Issue

Fetch a GitHub issue and implement what it describes, following the existing patterns of the codebase. Do not write tests — that is a separate step.

## Process

### 1. Fetch the issue

Run `gh issue view <number>` to get the issue title, description, and acceptance criteria. If no issue number was provided, ask the user for one.

### 2. Study the existing patterns

Before writing any code, read the codebase to understand the conventions:

- Read at least one existing Service (e.g., `RoomService`) and its corresponding Controller.
- Read at least one existing DTO pair (e.g., `RoomDto` / `CreateRoomDto`).
- Read the `AppDbContext` to understand the EF Core setup.
- Read `Program.cs` to understand dependency registration.

### 3. Implement in dependency order

1. **Domain changes** — new fields, enums, or entities if the issue requires them.
2. **DTO classes** — create or extend as needed.
3. **Service method** — add to the existing service or create a new one, following the interface pattern.
4. **Controller action** — wire the service, return the correct HTTP status code.
5. **Register in `Program.cs`** — add any new service registrations if needed.

### 4. Quality checklist

Before finishing, verify:

- No business logic in the controller — only call the service and map the result.
- No raw domain objects returned from endpoints — always use DTOs.
- Consistent error handling: return `BadRequest`, `NotFound`, or `Conflict` to match existing controllers.
- No hardcoded IDs, magic strings, or unnecessary dependencies.
- Build succeeds: run `dotnet build` to verify.

### 5. Report

List each file created or modified with a one-line summary of the change.
