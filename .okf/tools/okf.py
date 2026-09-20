#!/usr/bin/env python3
"""Check this OKF bundle and regenerate map.md.

    python3 .okf/tools/okf.py          check + regenerate map.md
    python3 .okf/tools/okf.py --check  check only (pre-commit hook and CI)
    python3 .okf/tools/okf.py --fix    also rewrite index.md entries whose description
                                       drifted from the concept's frontmatter, and append
                                       entries for unlisted siblings

Enforces OKF v0.2 (https://github.com/GoogleCloudPlatform/open-knowledge-format/blob/main/SPEC.md)
plus this bundle's own conventions (the `# Concept types` section of index.md).
Exits 2 on any error, which blocks the commit in .githooks/pre-commit and fails CI.
"""
import os, re, sys, posixpath, pathlib, collections

ROOT = pathlib.Path(__file__).resolve().parent.parent
REPO = ROOT.parent
CHECK_ONLY = "--check" in sys.argv
FIX = "--fix" in sys.argv

FM       = re.compile(r'\A---\n(.*?)\n---\n', re.S)
LINK     = re.compile(r'\[([^\]\n]+)\]\(([^)\s]+)\)')
ENTRY    = re.compile(r'^\* \[([^\]\n]+)\]\(([^)\s]+)\)(?:\s+[-—]\s+(.*))?$')   # index.md bullet
FENCE    = re.compile(r'^\s*```')
PATHLIKE = re.compile(r'^(\.{0,2}/)?[a-z0-9._-]+/\S*\.md$')   # banned as link TEXT
DATE_H   = re.compile(r'^## \d{4}-\d{2}-\d{2}')
RESERVED = {"index.md", "log.md"}                              # OKF section 3.1

files = sorted(str(p.relative_to(ROOT)).replace(os.sep, "/") for p in ROOT.rglob("*.md"))
fset  = set(files)
errors, warns = [], []
meta  = {}


def read(f):
    return (ROOT / f).read_text(encoding="utf-8")


def write(f, text):
    (ROOT / f).write_text(text, encoding="utf-8", newline="\n")


def resolve(src, href):
    """Resolve a link target to a bundle-relative path. Accepts relative and /bundle-absolute (section 6.1)."""
    path = href.partition("#")[0]
    if path.startswith("/"):
        return posixpath.normpath(path.lstrip("/"))
    d0 = posixpath.dirname(src)
    return posixpath.normpath(posixpath.join(d0, path) if d0 else path)


def outside_fences(text):
    inside = False
    for line in text.split("\n"):
        if FENCE.match(line):
            inside = not inside
            continue
        if not inside:
            yield line


def unquote(v):
    v = v.strip()
    return v[1:-1] if len(v) > 1 and v[0] == v[-1] and v[0] in "\"'" else v


# ---- frontmatter ----
for f in files:
    text = read(f)
    m = FM.match(text)
    fm = m.group(1) if m else ""
    d = {}
    for k in ("type", "title", "description", "tags", "status", "resource", "okf_version"):
        r = re.search(rf'^{k}:\s*(.+)$', fm, re.M)
        if r:
            d[k] = unquote(r.group(1))
    meta[f] = d
    base = posixpath.basename(f)

    if base == "index.md":
        # section 8: no frontmatter, except okf_version at the bundle root (section 12)
        keys = set(re.findall(r'^([A-Za-z_]+):', fm, re.M))
        if m and (f != "index.md" or keys - {"okf_version"}):
            errors.append(f"{f}: index.md must not have frontmatter (only the root may carry okf_version)")
    elif base == "log.md":
        lines = text.split("\n")
        if not lines[0].startswith("# "):
            errors.append(f"{f}: log.md must start with a '# ' title")
        for h in [l for l in lines if l.startswith("## ")]:
            if not DATE_H.match(h):
                errors.append(f"{f}: log heading must be '## YYYY-MM-DD ...' -> {h}")
    else:
        if not m:
            errors.append(f"{f}: no YAML frontmatter")
        elif not d.get("type"):
            errors.append(f"{f}: frontmatter has no non-empty 'type'")
        for k in ("title", "description"):
            if not d.get(k):
                warns.append(f"{f}: no {k}")

    res = d.get("resource")
    if res and not res.startswith("http") and not (REPO / res).exists():
        errors.append(f"{f}: resource does not exist -> {res}")

# ---- type vocabulary: the `# Concept types` section of the root index ----
vocab, section = set(), False
for line in read("index.md").split("\n"):
    if line.startswith("# "):
        section = line.strip() == "# Concept types"
    elif section and line.startswith("* "):
        vocab.update(re.findall(r'`([^`]+)`', line))
if not vocab:
    errors.append("index.md: no '# Concept types' section listing the type vocabulary")
for f, d in meta.items():
    t = d.get("type")
    if vocab and t and t not in vocab:
        errors.append(f"{f}: type '{t}' is not in the vocabulary in index.md")

# ---- links ----
linked = set()
for f in files:
    for line in outside_fences(read(f)):
        for label, href in LINK.findall(re.sub(r'`[^`]*\]\([^`]*`', '', line)):   # skip `[Title](link)` examples in inline code
            if PATHLIKE.match(label) and f != "map.md":
                errors.append(f"{f}: link text is a path -> [{label}]({href}) (use the target's title)")
            if href.startswith(("http://", "https://", "#", "mailto:")):
                continue
            path = href.partition("#")[0]
            if "NNNN" in path or "<" in path:
                continue
            if not path.endswith(".md"):
                # source link (.cs, a folder): must exist in the repo so a rename cannot rot it silently
                if not (ROOT / posixpath.dirname(f) / path).exists():
                    errors.append(f"{f}: broken source link -> {href}")
                continue
            ab = resolve(f, href)
            linked.add(ab)
            if ab not in fset:
                errors.append(f"{f}: broken link -> {href}")
        for lit in re.findall(r'`\.okf/([a-z0-9._/-]+)`', line):
            if "<" not in lit and not lit.endswith("/") and not (ROOT / lit).exists():
                errors.append(f"{f}: prose path does not exist -> .okf/{lit}")

# ---- index.md coverage and entry descriptions (section 8) ----
for d in sorted({posixpath.dirname(f) for f in files}):
    idx = posixpath.join(d, "index.md") if d else "index.md"
    if idx not in fset:
        errors.append(f"directory without index.md: {d or '.'}")
        continue
    lines = read(idx).split("\n")
    entries = {}                                   # target -> line number
    for i, line in enumerate(lines):
        e = ENTRY.match(line)
        if not e:
            continue
        label, href, desc = e.groups()
        target = resolve(idx, href)
        entries[target] = i
        want = meta.get(target, {}).get("description")
        if target in fset and posixpath.basename(target) != "index.md" and want and (desc or "") != want:
            if FIX:
                lines[i] = f"* [{label}]({href}) - {want}"
            else:
                errors.append(f"{idx}: description for {href} differs from its frontmatter (run --fix)")
        if posixpath.basename(target) == "index.md" and not desc:
            warns.append(f"{idx}: subdirectory entry {href} has no description")

    missing_c = [s for s in files if posixpath.dirname(s) == d and posixpath.basename(s) not in RESERVED
                 and s not in entries]
    subdirs = sorted({posixpath.dirname(s) for s in files
                      if posixpath.dirname(posixpath.dirname(s)) == d and posixpath.dirname(s) != d})
    missing_d = [s for s in subdirs if posixpath.join(s, "index.md") not in entries]
    for section, items, fmt in (
        ("# Subdirectories", missing_d,
         lambda s: f"* [{posixpath.basename(s)}]({posixpath.basename(s)}/index.md) - "),
        ("# Concepts", missing_c,
         lambda s: f"* [{meta[s].get('title') or posixpath.basename(s)}]({posixpath.basename(s)}) - {meta[s].get('description', '')}"),
    ):
        if not items:
            continue
        if not FIX:
            errors.extend(f"{idx} does not list {posixpath.relpath(s, d or '.')}" for s in items)
            continue
        new = [fmt(s) for s in items]
        if section in lines:
            at = lines.index(section) + 1
            while at < len(lines) and not lines[at].startswith("# "):
                at += 1
            while lines[at - 1].strip() == "":
                at -= 1
        else:
            at, new = len(lines), ["", section, ""] + new
        lines[at:at] = new
    if FIX:
        write(idx, "\n".join(lines).rstrip("\n") + "\n")

# ---- orphans ----
for f in files:
    if f not in linked and f != "index.md":
        warns.append(f"{f}: not linked from anywhere")

# ---- map.md: regenerated, or in --check mode compared (a stale copy is an error) ----
lines = ["---", "type: Reference", 'title: "Bundle Map"',
         'description: "Generated - every file in this bundle with its type, description and tags, for one-read search."',
         "tags: [generated, search]", "status: stable", "---", "",
         "# Bundle Map", "",
         "Generated by `python3 .okf/tools/okf.py` - **do not edit by hand.** Regenerate it whenever you",
         "add, move or rename a file. One read makes the whole bundle greppable.", "",
         "| Path | Type | Description | Tags |", "|---|---|---|---|"]
for f in files:
    if f == "map.md" or posixpath.basename(f) in RESERVED:
        continue
    d = meta[f]
    desc = d.get("description", "-").replace("|", "\\|")
    tags = d.get("tags", "-").strip("[]")
    lines.append("| [%s](%s) | %s | %s | %s |" % (f, f, d.get("type", "-"), desc, tags))
generated = "\n".join(lines) + "\n"
if CHECK_ONLY:
    if "map.md" not in fset or read("map.md") != generated:
        errors.append("map.md is stale -> run: python3 .okf/tools/okf.py")
else:
    write("map.md", generated)

print(f"files: {len(files)}")
print("types:", dict(sorted(collections.Counter(
    d["type"] for d in meta.values() if d.get("type")).items(), key=lambda kv: -kv[1])))
if warns:
    print(f"\nwarnings ({len(warns)}):")
    for w in warns[:40]:
        print("  ", w)
if errors:
    print(f"\nERRORS ({len(errors)}):", file=sys.stderr)
    for e in errors[:40]:
        print("  ", e, file=sys.stderr)
    sys.exit(2)
print("\nOK")
