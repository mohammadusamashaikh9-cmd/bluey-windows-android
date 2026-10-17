# Repository agent guide

Applies to Codex, Freebuff CLI, OpenCode and AutoClaw operating here.
This is an **unofficial source-based Windows + Android port** of
[Bluey by Riley](https://github.com/rbrown101010/bluey-by-riley), pinned for
foundation work to `6d84d91456f3bf7ad9b31caf31f19710b0ba8328`.

## Start with the task

Check `git remote -v`, current branch, HEAD and working-tree status before edits.
Preserve unrelated work. Read [STATUS](tracking/STATUS.md) and
[HANDOFF](tracking/HANDOFF.md), then only the topic documents needed below.
Do not reset, switch branches or rewrite history to make a gate pass.

The initial foundation uses the existing `planning/foundation` branch. Its owner
explicitly authorized a validated commit, push and PR to `main`, without merge.
That authorization is specific to this task; future work needs its own scope.
No unattended pushes, releases or merges.

## Source and architecture boundaries

- Preserve `Mac/`, `iOS/`, `Shared/`, Apple build files and existing preview assets.
  Translate into separately authorized target-platform paths; do not reorganize
  originals as incidental cleanup. Keep upstream attribution and font licenses.
- C#/.NET for Windows and Kotlin/Compose for Android are **proposals**. Framework,
  renderer, supported OS versions and packaging await a dated decision and spike.
- There is no required hosted backend or cloud database. Read
  [ARCHITECTURE](project/ARCHITECTURE.md) and [data schema](project/PROTOCOL-DATA-SCHEMA.md).
- Label facts as source-observed, port-planned or target-unverified. A material
  scope change requires a dated entry in [DECISIONS](tracking/DECISIONS.md), with
  rationale, alternatives and consequences before implementation.

## Route by topic

| Task | Read / use |
| --- | --- |
| Translate a source component | [PORTING-MAP](project/PORTING-MAP.md), [source-porting skill](.agents/skills/bluey-source-porting/SKILL.md) |
| Face, pointer or motion | [DESIGN-FIDELITY](project/DESIGN-FIDELITY.md), [animation skill](.agents/skills/bluey-animation-fidelity/SKILL.md) |
| Networking or persistence | [schema](project/PROTOCOL-DATA-SCHEMA.md), [SECURITY](project/SECURITY.md), [protocol skill](.agents/skills/bluey-device-protocol/SKILL.md) |
| Milestone or handoff | [IMPLEMENTATION](project/IMPLEMENTATION.md), [QA](project/QA.md), [checkpoint skill](.agents/skills/bluey-checkpoint-qa/SKILL.md) |
| Tool choice | [TOOL-POLICY](project/TOOL-POLICY.md) |

## Fidelity, trust and cost

Preserve vector geometry, colors, coordinate transforms, spring integration,
timings, mood precedence and interruption behavior before cosmetic changes.
Use injected clocks/randomness in future deterministic tests; platform animation
defaults are not evidence of parity. Compare recordings as well as stills.

Legacy newline JSON is a compatibility reference, not a secure transport.
Do not expose token minting, screenshots, microphone data or action tools before
authenticated encrypted pairing. Discovery names are not identities. No silent
plaintext downgrade. Validate bounded inputs and fail closed on privilege errors.
Computer control and recording require explicit opt-in in the port. Confirm
consequential actions in application logic; model instructions alone are insufficient.

Never commit credentials, ephemeral tokens, private captures, transcripts or user
audio. Long-lived OpenAI credentials stay in protected desktop storage; check
current official API docs, availability and cost before separately authorized API
work. Do not run upstream demo hooks that invoke APIs as a documentation check.

## Work ownership and evidence

Use one active writer per file. Delegate only when requested/authorized, with
explicit non-overlapping paths, deliverables and evidence boundaries. OpenCode
is an alternative agent/editor, not a concurrent writer to the same scope.
AutoClaw requires confirmed runtime, provider and permissions before use.

Prefer cheap checks proportional to the change: links, fixture parsing,
`git diff --check`, then focused unit tests when implementation exists. No
toolchain installs, paid calls, new binaries or application execution in F0.
Never invent test results. Distinguish local/static, CI, artifact and real-device
evidence; an APK/EXE build is not a device or animation pass.

End each meaningful task by updating current [STATUS](tracking/STATUS.md), gate
evidence in [CHECKPOINTS](tracking/CHECKPOINTS.md), a short [HANDOFF](tracking/HANDOFF.md),
and an append-only [SESSION-LOG](tracking/SESSION-LOG.md) entry. Report changed scope,
checks/results, limitations and one exact next task. Do not mark a gate verified
until every exit check has observed evidence.
