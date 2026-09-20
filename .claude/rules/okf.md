---
paths:
  - ".okf/**"
---

# Knowledge Base Rules

`.okf/` is an [Open Knowledge Format](https://github.com/GoogleCloudPlatform/open-knowledge-format) v0.2 bundle.
**Invoke the `open-knowledge-format` skill before editing anything here.** Procedure and conventions:
[.okf/workflows/process/maintain-the-knowledge-base.md](../../.okf/workflows/process/maintain-the-knowledge-base.md).

Where this bundle deliberately differs from the skill's checklist:

- **Links are relative** (`../x.md`), not bundle-absolute — so they render on GitHub. Both forms are spec-legal.
- **`index.md` never has frontmatter** (root carries only `okf_version`) and is never a concept. A domain
  slice's concept file is `domains/<slice>/<slice>.md`.
- **`type` vocabulary** is the list under `# Concept types` in [.okf/index.md](../../.okf/index.md). Do not invent synonyms.

Before finishing: run `python .okf/tools/okf.py` (checks the bundle, regenerates `map.md`) and add a line to
[.okf/log.md](../../.okf/log.md). `.githooks/pre-commit` runs `--check` whenever `.okf/` is staged (installed by any
`dotnet build`); a blocked commit is unfinished work, not noise.
