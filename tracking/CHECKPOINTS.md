# Checkpoints and evidence

States: **planned** = defined, not started; **active** = scoped work in progress;
**blocked** = named dependency prevents exit; **verified** = every listed exit
check has observed evidence. Owner roles are accountable work scopes, not proof
that someone has been assigned or performed tests.

| Gate | State | Accountable scope | Exit evidence |
| --- | --- | --- | --- |
| Fork creation (separate) | Verified remote state; creation user-executed | Repository owner | [Fork URL](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android); GitHub metadata confirms parent `rbrown101010/bluey-by-riley`, default `main`; both remote mains resolved to `6d84d91456f3bf7ad9b31caf31f19710b0ba8328` on 2026-10-18. Creation action itself was not performed/observed by this agent. |
| F0 foundation | Verified static foundation | Foundation author/reviewer | [Canonical navigation](../PORTING.md), [map](../project/PORTING-MAP.md), [rules](../AGENTS.md), four skills; 23 Markdown files / 173 local links, four frontmatters, six JSONL packets, 25-path allowlist, source/README preservation and baseline whitespace passed. Commands/limits in [session log](SESSION-LOG.md). Published for review in [PR #1](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/1); not merged. |
| F1 Android vector/animation | Planned | Android implementation + visual QA | [Plan](../project/IMPLEMENTATION.md), [visual authority](../project/DESIGN-FIDELITY.md), deterministic and physical-device comparisons in [QA](../project/QA.md); no execution |
| F2 Windows transparent overlay | Planned | Windows implementation + visual QA | Pure-engine tests, click-through/DPI/frame pacing and Windows recordings; no execution |
| F3 secure device link | Planned | Protocol/security implementation + cross-device QA | Auth/encryption/schema/negative tests plus real-device sync; no execution |
| F4 voice/context/privacy | Planned | Voice/context implementation + privacy QA | Current API review, authorized live run, permissions/capture/OCR/pointing and disconnect tests; no execution |
| F5 optional control/notes | Planned, optional | Scoped feature owner + safety/data QA | Application confirmation/stop or bounded recording/transcription/retention tests as selected; no execution |
| F6 release/demo | Planned | Release owner + artifact/device QA | Reproducible builds, hashes, CI/device evidence and truthful demo/docs; no execution |

No provisional ZIP/C0/C1 records were present in the checkout. No legacy gate is
counted as complete. If that kit is later supplied, retain its IDs only as aliases
to F0–F6 after checking actual scope/evidence; do not assume `C1` means `F1`.
Never double count fork creation as F0 implementation or infer completion from
an alias. [STATUS](STATUS.md) holds current verified facts; this table owns gate state.
