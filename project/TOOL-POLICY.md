# Practical agent and tool policy

Default: native Git, text search and the selected language toolchains once their
gate is authorized. Prefer **0–2 active MCPs**, chosen for the current task rather
than preinstalling a catalogue. F0 installs/configures no tools or global MCPs.

| Gate / use case | Default tools | Optional connector activation | Boundary |
| --- | --- | --- | --- |
| F0 source/docs/tracking | Git, PowerShell, rg, existing Python for static validation | GitHub MCP only if useful for metadata/PR operations | CLI Git/gh is sufficient; no app execution |
| F1 Android face | Approved Android/Kotlin/Gradle tools, emulator then physical device, local captures | Usually none; official Android docs via browser | SDK/toolchain setup needs implementation scope |
| F2 Windows overlay | Approved .NET/native UI tools, local profiler/capture | Microsoft Learn docs MCP for API lookup | Validate transparency/DPI/capture behavior on Windows |
| F3 secure local link | Native networking/TLS libraries, unit fixtures and owned test devices | Microsoft docs if Windows API detail is needed | No public relay, no unauthenticated live devices |
| F4 voice/context | Mocks first; native audio/capture; current official API docs | OpenAI docs MCP; Microsoft docs MCP if needed | No paid call until explicitly authorized; recheck model/access/cost |
| F5 optional notes/control | Native tests, test accounts/data and local evidence | Only a specific documented need, usually none | No blanket automation agent or elevated control |
| F6 build/PR/demo | Git, native build/signing workflow, CI and artifact hashing | GitHub MCP for issues/PR checks if CLI is insufficient | No autonomous publish/merge/release |

Official documentation checked on **2026-10-18 (Asia/Karachi)**:

- [Microsoft Learn MCP reference](https://learn.microsoft.com/en-us/training/support/mcp-developer-reference)
  lists the documentation endpoint [learn.microsoft.com/api/mcp](https://learn.microsoft.com/api/mcp).
- [OpenAI Docs MCP](https://developers.openai.com/resources/docs-mcp)
  documents [developers.openai.com/mcp](https://developers.openai.com/mcp).

These are documentation services, not application dependencies or hosted backend
requirements. Recheck official client/server setup and transport before using
third-party commands. No MCP setup command is prescribed here. API model names,
account availability, pricing and retention were not live-validated for this task.

## Writers and permissions

Codex, Freebuff CLI and OpenCode follow [AGENTS](../AGENTS.md). Choose one writer
for a file/slice. OpenCode is an alternative editor/agent, not a simultaneous code
writer. Narrow non-overlapping reviews/tasks may be delegated when authorized,
with owned paths and expected evidence. AutoClaw is permitted only after its
exact runtime/provider, file/network permissions and task boundary are confirmed;
do not infer credentials or configure it from a product name.

JADX, Ghidra and REA are unnecessary for this source-based port. Do not add binary
analysis tools, paid API wrappers, cloud databases, hosted backends or MCP servers
as substitutes for reading source. A missing optional tool is not a reason to
install a dependency when native Git/static validation can complete the work.
