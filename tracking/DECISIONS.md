# Decision log

Date basis: Asia/Karachi. Accepted planning decisions do not imply implemented
behavior. Add dated entries for material changes; preserve earlier decisions.

| ID / date | State | Decision, rationale, alternatives and consequence |
| --- | --- | --- |
| D001 / 2026-10-18 | Accepted | Preserve pinned Apple source and existing assets; source-based unofficial port with Riley attribution. Alternative: rewrite original directories or reverse-engineer binaries. Separate target code will require later authorization; no binary analysis needed. |
| D002 / 2026-10-18 | Accepted | No required hosted backend/cloud database in V1. Local settings/sessions/trust plus LAN protocol and direct OpenAI boundary match source ownership. Alternative: hosted relay/accounts/database adds operational and privacy scope without demonstrated need. Revisit only for a specific future requirement. |
| D003 / 2026-10-18 | Proposed | C#/.NET Windows and Kotlin/Compose Android; WPF/WinUI/renderer, SDK/OS/CPU support remain undecided. Alternatives include other native rendering stacks. F1/F2 spikes must prove fidelity, transparency/lifecycle and maintenance costs before acceptance. No packages/toolchains selected in F0. |
| D004 / 2026-10-18 | Accepted requirement; profile proposed | Authenticated encrypted device pairing before sensitive traffic, no plaintext downgrade; new version/service boundary distinct from legacy v0. Alternative: carry raw `_googly._tcp` trust forward. That exposes token/capture/action authority. Exact TLS/bootstrap/envelope and discovery identity await F3 review. |
| D005 / 2026-10-18 | Accepted | V1 is F1–F4; F5 control/notes optional, web research deferred. Separate mic/cloud/screen/recording/control opt-ins; computer control off by default, app-enforced consequential confirmation and latched stop if F5 selected. Alternative: reproduce upstream defaults/prompt-only rules; rejected for port trust boundary. |
| D006 / 2026-10-18 | Accepted | One canonical root/project/tracking tree and exactly four `.agents/skills` workflows. F0 is documentation/fixture validation only. Alternative: parallel planning kit tree/tool installations. No archive was present; future imported references must merge into this tree. |

## Open decisions for implementation

F1: Android minimum/target SDK, devices and renderer/font proof. F2: Windows
framework/renderer, supported OS/CPU and packaging. F3: TLS/bootstrap profile,
service/envelope version, limits/media strategy and trust recovery. F4: current
API/account/model/cost/retention and capture-consent UX; explicit storage migration
version/units. F5: whether either optional feature is selected and retention budget.
These are bounded gate inputs, not blockers to the documentation foundation.
