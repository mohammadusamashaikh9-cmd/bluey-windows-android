---
name: bluey-checkpoint-qa
description: Assess Bluey milestone evidence, triage failures and produce reproducible gate tracking and handoffs after scoped documentation, implementation or QA work.
---

# Evidence and checkpoint workflow

Read [CHECKPOINTS](../../../tracking/CHECKPOINTS.md), the selected gate's
[implementation exit checks](../../../project/IMPLEMENTATION.md) and relevant
[QA cases](../../../project/QA.md). Confirm branch, HEAD and changed paths;
preserve unrelated work. Gate states describe evidence, not effort spent.

For each exit check record exact command/result, tested commit, fixture/seed,
platform/runtime or device/OS and evidence path/hash as appropriate. Keep static
source inspection, local tests, CI, artifact inspection and real-device/manual
comparison in separate rows. Never promote a build or mock pass into a device,
performance or privacy acceptance result. Mark unrun checks and blockers explicitly.

On failure, capture minimal reproduction, expected/source authority, observed
behavior, severity and affected gate. Choose the smallest diagnostic experiment
before proposing a fix. Avoid rerunning broad suites after a focused docs change
when link/fixture/frontmatter/whitespace checks answer the question.

Update current [STATUS](../../../tracking/STATUS.md) and
[HANDOFF](../../../tracking/HANDOFF.md); append a meaningful dated entry to
[SESSION-LOG](../../../tracking/SESSION-LOG.md). Put new architecture/scope choices
in [DECISIONS](../../../tracking/DECISIONS.md), not hidden in a test report.
Keep one exact next task, its source owner, authorized boundary and acceptance.

F0 checks relative links, four skill frontmatters, JSONL hygiene, unchanged original
sources and `git diff --check`; no target toolchains or app execution. Use the
installed skill validator only if dependencies already exist; report an unavailable
validator separately from a dependency-free fallback. Commit/push/PR only within
task authorization. Report head/base/URL after verification; never merge or release
from an evidence handoff alone.

For this pinned foundation run `python .agents/skills/bluey-checkpoint-qa/scripts/validate_foundation.py`
from the repository root. The [static helper](scripts/validate_foundation.py) uses
only the standard library and checks an explicit foundation allowlist; adapt that
allowlist/baseline deliberately for a later authorized scope. Its frontmatter
check supports these two plain scalar fields, not arbitrary YAML or skill behavior.
