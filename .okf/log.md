# Knowledge Base Update Log

## 2026-09-20 (remove engineering/code)

* **Removal**: Deleted `engineering/code/` (123 one-file-per-type concepts) - it mirrored the source 1-1,
  which AGENTS.md forbids. The rules the richer pages carried now live with their owners
  ([SharedKernel Layer](architecture/components/shared-kernel.md), [Persistence](architecture/cross-cutting/persistence.md),
  [Domain Event](engineering/patterns/domain-event.md), [Encryption](engineering/patterns/encryption.md),
  [Decorator Pipeline](architecture/cross-cutting/decorator-pipeline.md), [API Surface](architecture/cross-cutting/api-surface.md),
  [Security and Authorization](architecture/cross-cutting/security-and-authorization.md),
  [Test Architecture](architecture/delivery/test-architecture.md)); every other link now points at the `.cs`
  file or folder. `Code` and `Abstraction` left the type vocabulary.
* **Rewrite**: [Pattern](engineering/patterns/index.md) pages now state the rule and show only the
  distinctive lines, linking a real instance in `src/` instead of pasting whole classes (endpoint page went
  from 62 to 7 code lines). [Pattern Template](engineering/patterns/_template-pattern.md) codifies this.
* **Fix**: handler files are `<Operation>Command.cs` / `<Operation>Query.cs` (record + handler), as the code
  has always had it - [Naming](engineering/conventions/naming.md) and
  [Add a Feature](workflows/engineering/add-a-feature.md) said `...Handler.cs`.
* **Merge**: One topic, one page. `engineering/constraints/` (12 files, 5 of them "not test-enforced")
  collapsed into the table [Constraints](engineering/constraints.md); each rule's *why* moved to its owner
  (pattern, convention or cross-cutting page). `file-placement` + `abstraction-placement` + `naming` became
  [Naming and Placement](engineering/conventions/naming.md); `lock-names` -> [Distributed Lock](engineering/patterns/distributed-lock.md);
  `cache-keys` -> [Cached Read](engineering/patterns/cached-read.md); `ioptions-only` +
  `cross-cutting/configuration-and-options` -> [Options Pattern](engineering/patterns/options-pattern.md).
  Repeated passages (record syntax, ExecuteUpdate consequences, the concurrency decision table, the api/v1
  group) now appear once and are linked from everywhere else.
* **Restructure**: [Glossary](glossary/index.md) is flat - `glossary/<term>.md`, the subject as the first
  tag, and one `index.md` with a `#` section per subject - instead of nine sub-folders each holding a
  handful of terms. The `glossary` tag (noise: it equalled the directory) is gone.
* **Tooling**: `okf.py` now verifies that relative non-`.md` links (source files, folders) exist, so a rename
  cannot rot them silently.

## 2026-09-20 (OKF v0.2 conformance)

* **Restructure**: Brought every `index.md` into the shape the
  [OKF v0.2 spec](https://github.com/GoogleCloudPlatform/open-knowledge-format/blob/main/SPEC.md) reserves for
  it - no frontmatter (the root keeps only `okf_version`), a short lead, then `#` sections of
  `* [Title](link) - description` bullets whose descriptions are copied from the target's frontmatter. The
  pseudo-types `Index` and `Knowledge Bundle` are gone; the vocabulary now lives under `# Concept types` in
  [the root index](index.md) and the checker rejects anything else.
* **Restructure**: A domain slice's concept is now `domains/<slice>/<slice>.md`
  ([Audit Logs](domains/audit-logs/audit-logs.md), [Emails](domains/emails/emails.md),
  [Outbox](domains/outbox/outbox.md)); `index.md` is never a concept.
* **Creation**: [Troubleshooting by Symptom](troubleshooting.md) (the root symptom table) and
  [Maintain the Knowledge Base](workflows/process/maintain-the-knowledge-base.md), which absorbs the
  "adding one" / "changing one" guidance that used to sit in twelve indexes.
* **Update**: [`tools/okf.py`](map.md) now enforces the reserved-file rules, the type vocabulary and
  index-entry descriptions, gains `--fix`, and exits 2 so the new PostToolUse hook in `.claude/settings.json`
  and the CI step surface failures to the agent. [Bundle Map](map.md) is `type: Reference`.
* **Update**: Vendored the `open-knowledge-format` skill into `.claude/skills/` and added
  `.claude/rules/okf.md`, which routes agents to it and records this bundle's two deliberate exceptions:
  relative links (so GitHub renders them) and the slice-file convention above.
* **Fix**: Repointed the upstream URL to `GoogleCloudPlatform/open-knowledge-format`.
* **Update**: Moved the `okf.py --check` gate and `dotnet format` from Claude Code PostToolUse hooks to
  `.githooks/pre-commit`, which formats only the staged `.cs` files and checks `.okf/` only when it is staged. `Directory.Build.props` sets `core.hooksPath` on every build so each
  clone has the hook without a setup step; `--check` now also fails on a stale [Bundle Map](map.md). See
  [Maintain the Knowledge Base](workflows/process/maintain-the-knowledge-base.md).

## 2026-09-20 (regroup)

* **Restructure**: Moved the five code-level directories — `code/`, `patterns/`, `technologies/`,
  `conventions/`, `constraints/` — under [engineering/](engineering/index.md), so the root lists *kinds of
  question* (architecture, domains, decisions, engineering, workflows, glossary, backlog) rather than
  mixing altitudes. Every relative link was re-resolved by script and verified with `tools/okf.py`.
* **Restructure**: Split `workflows/` by area into [engineering/](workflows/engineering/index.md) (build,
  run, test, migrate, format) and [process/](workflows/process/index.md) (branching and PRs, extending the
  template). `runbooks/` is the reserved name for human-only procedures (secrets, accounts) and is not
  created until the first one exists.
* **Deliberately unchanged**: `glossary/` keeps its nine flat subject groups (`glossary/domain/` is added
  as a sibling when the first business term arrives); `tools/` has one file and gets no sub-folder yet.
* **Update**: Repointed `README.md`, `AGENTS.md` and `.claude/rules/*.md` at the new paths.

## 2026-09-20 (review pass)

* **Fix**: Rewrote 313 link labels that still named the pre-restructure directories (`pattern/`,
  `constraint/`, `system/`, `component/`). Labels are now the target's `title`.
* **Fix**: Corrected the "how to add a file" instructions in
  [Add a Feature](workflows/engineering/add-a-feature.md), [Extending the Template](workflows/process/extending-the-template.md),
  `Code Template` and [Term Template](glossary/_template-term.md) — they pointed at
  flat paths and the wrong `type`.
* **Creation**: Added [`tools/okf.py`](map.md), a checker that validates frontmatter, links, `resource:`
  paths, index coverage and banned path-shaped link labels, and regenerates [Bundle Map](map.md).
* **Creation**: Added a symptom-indexed table to [the bundle index](index.md), and
  [Bundle Map](map.md) so the whole bundle is greppable in one read.
* **Update**: Gave every `index.md` frontmatter; annotated the nine glossary group indexes from each term's
  description; stated the cross-cutting-vs-pattern rule; added an **Enforced by** section to every
  constraint, including the seven that no test covers.
* **Update**: Added `.claude/rules/infrastructure.md` and widened the endpoint rules to all of
  `src/Web.Api/**`.

## 2026-09-20

* **Restructure**: Reorganised the bundle around the [C4 model](https://c4model.com). `system/` became
  `architecture/`, split into `context.md` (L1), `containers/` (L2), `components/` (L3),
  `cross-cutting/` and `delivery/`. `component/` became `code/` (L4), grouped into twelve folders by the
  role a type plays.
* **Creation**: Added C4 level 1 and level 2, which did not exist before —
  [System Context](architecture/context.md) and
  [architecture/containers/](architecture/containers/index.md), recording that this is **one system with
  one deployed container** and what follows from that.
* **Update**: Pluralised the remaining directories (`patterns/`, `conventions/`, `constraints/`,
  `workflows/`, `technologies/`, `domains/`); `adr/`, `glossary/` and `backlog/` kept their names.
* **Update**: Gave `domains/` a folder per slice and `glossary/` nine subject groups; every directory now
  has its own `index.md`.
* **Update**: Repointed `README.md`, `AGENTS.md` and `.claude/rules/*.md` at the new paths.
* **Removal**: Deleted the `docs/` tree it replaced.

## 2026-09-19

* **Initialization**: Created the OKF v0.2 bundle at `.okf/`, replacing the previous `docs/` tree.
* **Migration**: Decomposed `docs/Architecture.md`, `docs/VerticalSliceStructure.md`,
  `docs/FeatureTemplates.md`, `docs/DevelopmentGuideline.md`, `docs/Caching.md`, `docs/Concurrency.md`,
  `docs/DistributedLock.md`, `docs/DomainEvent.md`, `docs/OutboxPattern.md`, `docs/Encryption.md`,
  `docs/AuditLogging.md` and `docs/PendingTasks.md` into one concept per noun.
* **Creation**: Established [adr/](adr/index.md) by reconstructing the decisions the previous docs implied.
* **Update**: Reduced `CLAUDE.md` and `AGENTS.md` to a minimal entry point that references this bundle.
