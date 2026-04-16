---
name: multi-perspective-review
description: Run a parallel code review across correctness, quality, security, and architecture.
agent: Thorough Reviewer
tools: ['agent', 'read', 'search']
argument-hint: What should be reviewed? Example: the pending changes, a specific file, or the reservation feature.
---

Review this target using the Thorough Reviewer workflow:

${input:reviewTarget:the pending changes or specific files}

Instructions:

- Run the four review perspectives in parallel.
- Keep the perspectives independent before synthesis.
- Prioritize real findings over generic suggestions.
- Output findings first, with severity and file references.
- If there are no findings, say so explicitly and mention remaining review limits.