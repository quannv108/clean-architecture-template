---
type: Workflow
title: "Format Code"
description: "Checking and fixing style violations without touching unrelated files."
tags: [workflow, formatting, style, dotnet-format]
status: stable
---

# Format Code

```bash
# Check only
dotnet format CleanArchitecture.slnx style --verify-no-changes --severity error

# Fix
dotnet format CleanArchitecture.slnx style

# Limit the check output to files you touched
dotnet format CleanArchitecture.slnx style --verify-no-changes --severity error 2>&1 \
  | grep "src/Application/MyFeature"
```

**Only fix violations in files you created or modified.** The solution may carry pre-existing violations
elsewhere, and reformatting them buries your change in an unreviewable diff.

Rules come from `.editorconfig`. The pre-commit hook (`.githooks/pre-commit`, installed by any `dotnet build`)
runs `dotnet format --include <staged .cs files>` and re-stages them, so a commit is formatted without touching
files you did not change. It refuses a partially staged `.cs` file rather than staging your unstaged hunks -
stage the whole file or stash the rest. See [Maintain the Knowledge Base](../process/maintain-the-knowledge-base.md)
for how the hook is installed.

The same check runs in [CI](../../architecture/delivery/ci-pipeline.md).
