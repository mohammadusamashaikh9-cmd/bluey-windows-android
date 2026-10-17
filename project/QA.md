# QA and evidence plan

F0 is static foundation validation. F1–F6 test plans below have **not run**.
See [CHECKPOINTS](../tracking/CHECKPOINTS.md) for gate ownership/evidence.

## Evidence rules

Record date/time zone, commit, exact command and exit/result, tool/runtime version,
input fixture/seed, platform/device/OS, display/DPI/network setup and artifact
location/hash where relevant. Redact user data. Distinguish observed output from
manual interpretation. A checklist, mock test, CI build or screenshot is not
evidence of another tier. Link failures and state why a check was not run.

## F0 checks

- Verify local branch/HEAD/remotes/status and remote fork parent/default branch.
  Compare the initial tracked tree to the pin; document any divergence.
- Inspect canonical source owners listed in [PORTING-MAP](PORTING-MAP.md). Review
  claims about transport security, model/API assumptions, dates, animation and
  data ownership against actual code. Verify original directories unchanged.
- Enumerate canonical Markdown and skill links; resolve relative files and
  heading anchors where feasible. Check fenced blocks are not treated as links.
  External URLs are not all required to be fetched; identify what was checked.
- Parse each synthetic JSONL fixture line as strict JSON; verify fixture keys and
  valid legacy face values. This is fixture hygiene, not a working codec test.
- Check skill frontmatter (`name`, `description`, folder match, hyphenated names),
  no empty scaffold instructions, and exactly four requested skills. Use bundled
  `quick_validate.py` if its existing runtime dependencies are available; otherwise
  document the limitation and use a small dependency-free equivalent check.
- Run `git diff --check` and review the exact staged path list. After commit,
  check its diff for whitespace and verify push/PR head and base independently.

Exact foundation command results and limitations are recorded in
[SESSION-LOG](../tracking/SESSION-LOG.md). No app/build/device/CI acceptance in F0.

Reproduce the static checks from the repository root with
`python .agents/skills/bluey-checkpoint-qa/scripts/validate_foundation.py`.
The [standard-library helper](../.agents/skills/bluey-checkpoint-qa/scripts/validate_foundation.py)
checks links, the restricted two-field YAML form, strict fixture JSON, an explicit
path allowlist, preserved source/README body and baseline whitespace. It does not
fetch external URLs, compile native code or prove skill behavior. Stage reviewed
files and run it again so Git also checks whitespace in new files.

## Planned deterministic and cross-language tests

| Area | Meaningful cases |
| --- | --- |
| Face math | Known initial state and dt; spring update order; all seven moods; local/remote precedence; stale input; blink double phases; fixed RNG; bounds and large frame gaps |
| Cursor math | TimingCurve inversion/derivative endpoints; flight positions/velocity continuity; redirect mid-flight; one landing; dock/follow fade; press/hover; DPI coordinate round-trips |
| Codec | Byte-by-byte fragmentation, multiple lines/read, multibyte UTF-8 split, escaped newline, malformed JSON, missing face fields, unknown mood, empty/oversized line, invalid base64, EOF mid-frame |
| Correlation/security | Unknown/mismatched/duplicate/late replies, timeout/cancel, bounded pending work, incompatible versions, unknown commands, bad cert/key, expired enrollment, replay/revoke/downgrade |
| Storage | Explicit timestamp epochs/units, atomic crash recovery, corrupt file, migration, stable IDs, pending audio retry/idempotency, export and delete boundaries |
| Voice/context | Permission denial, empty commit, held gesture during connect, text/chirp state, late events after sleep, auth failure, offline reconnect, stale snapshot/ID and capture refusal |
| Optional control/notes | Unapproved effects, injection, secure field, cancel during awaited animation/drag, no replay on reconnect; path traversal, bounded upload and retention/delete |

Use the same language-neutral fixtures in future C# and Kotlin tests. Add negative
fixtures with explicit expected rejection; don't treat source's silent invalid-line
discard as the target security policy. Protocol fuzz tests must respect offline
scope and never contact discovered devices without an authorized test plan.

## Planned CI and artifacts

When build setup is authorized, run inexpensive lint/unit/codec tests on changes,
then native builds in suitable Windows/Android jobs. Pin runtime versions and
report test counts/failed cases. Signing secrets remain in protected CI settings;
never expose them in PR artifacts. CI configuration is future work. F6 records a
single deterministic artifact per target/config with SHA-256 and ties device QA
to that exact digest. Release/signing/upload are separate acceptance boundaries.

## Planned manual device and visual comparisons

For each capture, save source/target commit, resolution/aspect, density/DPI, font
hashes, refresh rate, device/OS, seed/input trace and time base. Obtain runnable
Apple reference recordings when possible; upstream previews are reference-only.
Use fixed frames for all moods, gaze extremes, closed/squint eyes, talk levels,
cropped body/crown, gradients and caption lengths. Overlay masks/differences and
inspect highlight/clip/anti-aliasing differences. Agree tolerances before approval.

Record transitions at 60 fps where supported: mood-change blink/hop, idle to
sleepy/resting, interrupted flight, home/return, comet/string trails, speech bubble
appear/read/clear. Compare onset, trajectory, settling and frame pacing, not only
end states. Test Android physical landscape/lifecycle and Windows 100/150/200%
DPI, hotkey conflicts and click-through. Measure CPU/battery/thermal behavior and
report conditions; F0 provides no performance measurements.

F3 additionally needs real LAN discovery/auth/reconnect and synchronized face tests
on Windows plus Android. F4 needs explicit live API authorization and current docs;
use mock boundaries until then. F5 needs stop without network/model cooperation.
File a triage record with reproduction, expected/source authority, actual result,
severity, affected gate, evidence and minimal next experiment; no synthetic success.
