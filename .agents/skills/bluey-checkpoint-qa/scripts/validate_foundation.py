"""Static F0 checks; standard library only. Does not run or build applications."""

import json
import re
import subprocess
from pathlib import Path
from urllib.parse import unquote, urlsplit


ROOT = Path(__file__).resolve().parents[4]
BASE = "6d84d91456f3bf7ad9b31caf31f19710b0ba8328"
SKILLS = {
    "bluey-source-porting", "bluey-animation-fidelity",
    "bluey-device-protocol", "bluey-checkpoint-qa",
}
DOCS = ["AGENTS.md", "README.md", "PORTING.md"] + [
    "project/" + name + ".md" for name in (
        "PRD", "TRD", "ARCHITECTURE", "APP-FLOWS", "DESIGN-FIDELITY",
        "PROTOCOL-DATA-SCHEMA", "SECURITY", "PORTING-MAP", "IMPLEMENTATION",
        "QA", "TOOL-POLICY",
    )
] + ["tracking/" + name + ".md" for name in (
    "STATUS", "CHECKPOINTS", "DECISIONS", "HANDOFF", "SESSION-LOG",
)]
DOCS += [f".agents/skills/{name}/SKILL.md" for name in sorted(SKILLS)]
FIXTURE = ".agents/skills/bluey-device-protocol/references/legacy-packets.jsonl"
HELPER = ".agents/skills/bluey-checkpoint-qa/scripts/validate_foundation.py"
ALLOWED = set(DOCS + [FIXTURE, HELPER])


def require(condition, message):
    if not condition:
        raise ValueError(message)


def git(*args):
    result = subprocess.run(
        ["git", *args], cwd=ROOT, check=True, capture_output=True, text=True,
        encoding="utf-8",
    )
    return result.stdout


def outside_fences(text):
    return re.sub(r"(?ms)^```[^\n]*\n.*?^```\s*$", "", text)


def slug(heading):
    heading = re.sub(r"[^\w\- ]", "", heading.lower())
    return heading.replace(" ", "-")


def unique_object(pairs):
    value = {}
    for key, item in pairs:
        require(key not in value, "Duplicate JSON key: " + key)
        value[key] = item
    return value


def reject_constant(value):
    raise ValueError("Non-finite JSON constant: " + value)


def parse_json(value):
    return json.loads(value, object_pairs_hook=unique_object,
                      parse_constant=reject_constant)


def main():
    for name in DOCS:
        require((ROOT / name).is_file(), "Missing canonical file: " + name)
    actual = {p.name for p in (ROOT / ".agents/skills").iterdir() if p.is_dir()}
    require(actual == SKILLS, "Expected exactly the four project skills")

    links = 0
    external = 0
    for name in DOCS:
        path = ROOT / name
        body = outside_fences(path.read_text(encoding="utf-8"))
        for target in re.findall(r"!?\[[^\]\n]*\]\(([^)\n]+)\)", body):
            target = target.strip().strip("<>")
            parts = urlsplit(target)
            if parts.scheme or parts.netloc:
                external += 1
                continue
            resolved = (path.parent / unquote(parts.path)).resolve()
            require(resolved.is_relative_to(ROOT), f"Link escapes repo: {name}: {target}")
            require(resolved.exists(), f"Broken link: {name}: {target}")
            if parts.fragment:
                headings = re.findall(r"(?m)^#{1,6}\s+(.+)$", resolved.read_text(encoding="utf-8"))
                require(unquote(parts.fragment) in {slug(h) for h in headings},
                        f"Missing anchor: {name}: {target}")
            links += 1
    print(f"PASS: {len(DOCS)} Markdown files; {links} local links resolved; "
          f"{external} external link occurrences not fetched by this checker")

    for name in sorted(SKILLS):
        text = (ROOT / f".agents/skills/{name}/SKILL.md").read_text(encoding="utf-8")
        match = re.match(r"\A---\nname: ([a-z0-9-]+)\ndescription: ([^\n]+)\n---\n", text)
        require(match is not None, "Expected two plain YAML scalar fields: " + name)
        require(match[1] == name and len(name) < 64, "Invalid skill name: " + name)
        description = match[2]
        require(0 < len(description) <= 1024 and ": " not in description
                and " #" not in description and description[0].isalnum()
                and not any(c in description for c in "[]{}<>\t"),
                "Unsupported plain YAML description: " + name)
        require("TODO" not in text and len(text[match.end():].strip()) > 100,
                "Empty or unfinished skill: " + name)
    print("PASS: 4 skill frontmatters, folder names and nonempty instructions "
          "(restricted plain-scalar check; not the PyYAML validator)")

    lines = (ROOT / FIXTURE).read_bytes()
    require(lines.endswith(b"\n"), "Fixture requires final LF")
    packets = [parse_json(line) for line in lines.decode("utf-8").splitlines()]
    fields = {"face", "hello", "volume", "command", "audio", "speech", "callID", "tool", "text", "image"}
    for packet in packets:
        require(isinstance(packet, dict) and set(packet) <= fields,
                "Fixture contains unknown legacy fields")
        if "face" in packet:
            face = packet["face"]
            require(set(face) == {"gazeX", "gazeY", "mood", "talk"}, "Invalid face keys")
            require(face["mood"] in {"listening", "resting", "thinking", "talking", "pointing", "happy", "sleepy"}, "Invalid mood")
            require(all(type(face[k]) in (int, float) for k in ("gazeX", "gazeY", "talk")), "Invalid face number")
            require(-1 <= face["gazeX"] <= 1 and -1 <= face["gazeY"] <= 1
                    and 0 <= face["talk"] <= 1, "Invalid face range")
        if packet.get("command") == "tool":
            require(isinstance(parse_json(packet["text"]), dict), "Invalid tool args")
    print(f"PASS: {len(packets)} strict JSONL packets, legacy keys, face values and nested tool JSON")

    changed = set(git("diff", "--name-only", BASE).splitlines())
    changed.update(git("ls-files", "--others", "--exclude-standard").splitlines())
    require(changed <= ALLOWED, "Out-of-scope changed paths: " + repr(sorted(changed - ALLOWED)))
    protected = ["Mac", "iOS", "Shared", "docs", "GooglyEyes.xcodeproj",
                 "Package.swift", "project.yml", "LICENSE", ".gitignore", "scripts"]
    require(not git("diff", "--name-only", BASE, "--", *protected).strip(),
            "Original source/build/assets changed")
    original = git("show", BASE + ":README.md")
    current = (ROOT / "README.md").read_text(encoding="utf-8")
    require(current.split("## Original Mac + iPhone project\n\n", 1)[1]
            == original.split("\n\n", 1)[1], "Original README body changed")
    git("diff", "--check", BASE)
    print(f"PASS: {len(changed)} allowed changed paths; original source/build/assets "
          "and README body preserved; git diff --check BASE")
    print("Static foundation validation only; no app, build, CI or device test.")


if __name__ == "__main__":
    main()
