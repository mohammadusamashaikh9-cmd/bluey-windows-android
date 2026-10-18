# Windows + Android port foundation

This fork develops an unofficial source-based port. Android has an offline native
face demo under `android/`; Windows F2 adds a native offline overlay under `windows/`.
The Apple source remains
the unchanged baseline. See current tracking for build and device evidence.

Start with [current status](tracking/STATUS.md), [next task](tracking/HANDOFF.md)
and [agent rules](AGENTS.md). No toolchain installation is needed to review F0;
use [CI build instructions](project/QA.md#android-f1-build-and-comparison) for F1.

| Need | Canonical document |
| --- | --- |
| Product scope and acceptance | [PRD](project/PRD.md) |
| Requirements and platform uncertainties | [TRD](project/TRD.md) |
| Component boundaries | [Architecture](project/ARCHITECTURE.md) |
| User and device transitions | [App flows](project/APP-FLOWS.md) |
| Visual and motion authority | [Design fidelity](project/DESIGN-FIDELITY.md) |
| Local protocol and stored data | [Protocol/data schema](project/PROTOCOL-DATA-SCHEMA.md) |
| Pairing, privacy and actions | [Security](project/SECURITY.md) |
| Source-to-target mapping | [Porting map](project/PORTING-MAP.md) |
| Work order and exit checks | [Implementation](project/IMPLEMENTATION.md), [QA](project/QA.md) |
| Agent/tool selection | [Tool policy](project/TOOL-POLICY.md) |
| Gates, decisions, history | [Checkpoints](tracking/CHECKPOINTS.md), [decisions](tracking/DECISIONS.md), [session log](tracking/SESSION-LOG.md) |

Baseline: [upstream commit](https://github.com/rbrown101010/bluey-by-riley/commit/6d84d91456f3bf7ad9b31caf31f19710b0ba8328).
The optional `Bluey_Windows_Android_Project_Setup_v1.zip` was absent from this
worktree at foundation inspection. No competing planning set was found; these
documents are the single port planning tree. Existing `docs/previews/` stays as
upstream visual reference. Facts describe inspected source, not a running Apple app.
