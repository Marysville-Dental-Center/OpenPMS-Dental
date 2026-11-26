"""Checks the links of the repository's documents: every tracked Markdown file and the project site (site/index.html).

The same script runs in each of the office's repositories. A relative link must name a file or folder of the repository (tracked, or new and not ignored), and a #fragment must name a heading of the Markdown file it points to (GitHub's
heading anchors). Links to the project's own GitHub pages (blob/tree/main of this repository) are checked the same way. Other web
addresses are not fetched; --external checks them too (slow, needs the network; not part of the routine check).

Run from anywhere:  python -I tools/check_doc_links.py [--external]
Exit code 0 when every link is valid, 1 with one line per broken link.
"""
import os
import re
import subprocess
import sys
import urllib.parse
import urllib.request

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))



def repo_urls():
    """This repository's own GitHub file addresses (blob/tree on main), from the origin remote."""
    try:
        url = subprocess.run(["git", "-c", "safe.directory=*", "remote", "get-url", "origin"], cwd=ROOT, capture_output=True,
                             check=True, text=True).stdout.strip()
    except Exception:
        return ()
    url = re.sub(r"\.git$", "", url)
    url = re.sub(r"^git@github\.com:", "https://github.com/", url)
    return (url + "/blob/main/", url + "/tree/main/")


REPO_URLS = repo_urls()
# Folders that hold copies of third-party documents, whose links point into their own projects.
SKIP_PREFIXES = ("third-party/", "third_party/", "Required dlls/", "UnitTests/Resources/")

MD_LINK = re.compile(r"(?<!\!)\[(?:[^\[\]]|\[[^\]]*\])*\]\(\s*<?([^)\s>]+)>?(?:\s+\"[^\"]*\")?\s*\)")
MD_IMAGE = re.compile(r"!\[[^\]]*\]\(\s*<?([^)\s>]+)>?(?:\s+\"[^\"]*\")?\s*\)")
MD_REF = re.compile(r"^\s*\[[^\]]+\]:\s*<?(\S+?)>?(?:\s+.*)?$", re.M)
HTML_ATTR = re.compile(r"""(?:href|src)\s*=\s*["']([^"']+)["']""", re.I)
FENCE = re.compile(r"^(\s*)(```|~~~)")


def tracked_files():
    # Tracked files and new files not ignored, so a document is checked before its first commit.
    out = subprocess.run(["git", "-c", "safe.directory=*", "ls-files", "-z", "--cached", "--others", "--exclude-standard"], cwd=ROOT,
                         capture_output=True, check=True).stdout
    return [p for p in out.decode("utf-8").split("\0") if p]


def strip_code(text):
    """The text without fenced code blocks and inline code, which hold examples, not links."""
    lines, in_fence, marker = [], False, ""
    for line in text.split("\n"):
        m = FENCE.match(line)
        if m and (not in_fence or line.strip().startswith(marker)):
            in_fence, marker = (not in_fence), m.group(2)
            lines.append("")
            continue
        lines.append("" if in_fence else re.sub(r"`[^`\n]*`", "", line))
    return "\n".join(lines)


def slug(heading):
    """GitHub's anchor for a heading: lower case, markup and punctuation removed, spaces to hyphens."""
    h = re.sub(r"!\[[^\]]*\]\([^)]*\)", "", heading)
    h = re.sub(r"\[([^\]]*)\]\([^)]*\)", r"\1", h)
    h = re.sub(r"<[^>]+>", "", h).strip().lower()
    h = re.sub(r"[^\w\- ]", "", h, flags=re.UNICODE)
    return h.replace(" ", "-")


_anchor_cache = {}


def anchors(path):
    if path not in _anchor_cache:
        found, counts = set(), {}
        with open(path, encoding="utf-8-sig", errors="replace") as f:
            text = strip_code(f.read())
        for m in re.finditer(r"^#{1,6}\s+(.*?)\s*#*\s*$", text, re.M):
            s = slug(m.group(1))
            n = counts.get(s, 0)
            counts[s] = n + 1
            found.add(s if n == 0 else f"{s}-{n}")
        for m in re.finditer(r"""<a\s+(?:name|id)\s*=\s*["']([^"']+)["']""", text, re.I):
            found.add(m.group(1))
        _anchor_cache[path] = found
    return _anchor_cache[path]


def check_external(url):
    try:
        req = urllib.request.Request(url, method="HEAD", headers={"User-Agent": "doc-link-check"})
        with urllib.request.urlopen(req, timeout=15) as r:
            return r.status < 400
    except Exception:
        try:
            req = urllib.request.Request(url, headers={"User-Agent": "doc-link-check"})
            with urllib.request.urlopen(req, timeout=15) as r:
                return r.status < 400
        except Exception:
            return False


def main():
    external = "--external" in sys.argv
    files = tracked_files()
    tracked = set(files)
    folders = set()
    for p in files:
        parts = p.split("/")
        for i in range(1, len(parts)):
            folders.add("/".join(parts[:i]))
    docs = [p for p in files if (p.lower().endswith(".md") or p == "site/index.html") and not p.startswith(SKIP_PREFIXES)]
    broken = []
    for doc in docs:
        with open(os.path.join(ROOT, doc), encoding="utf-8-sig", errors="replace") as f:
            raw = f.read()
        if doc.endswith(".html"):
            targets = HTML_ATTR.findall(raw)
        else:
            text = strip_code(raw)
            targets = MD_LINK.findall(text) + MD_IMAGE.findall(text) + MD_REF.findall(text)
            targets += [t for t in HTML_ATTR.findall(text)]
        base = os.path.dirname(doc)
        for target in targets:
            t = target.strip()
            for prefix in REPO_URLS:
                if t.startswith(prefix):
                    t = "/" + t[len(prefix):]
                    break
            if re.match(r"^[a-z][a-z0-9+.-]*:", t, re.I) or t.startswith("//"):
                if external and t.lower().startswith(("http://", "https://")) and "__BASE_URL__" not in t:
                    if not check_external(t):
                        broken.append(f"{doc}: {target} (not reachable)")
                continue
            if "__BASE_URL__" in t or t.startswith("{"):
                continue
            path, _, frag = t.partition("#")
            path = urllib.parse.unquote(path.split("?")[0])
            if path == "":
                resolved = doc
            elif path.startswith("/"):
                resolved = os.path.normpath(path.lstrip("/")).replace("\\", "/")
            elif doc == "site/index.html":
                # The site serves site/ as its root, with the logo copied in by the workflow.
                resolved = os.path.normpath(os.path.join("site", path)).replace("\\", "/")
                if path in ("logo.svg", "logo-256.png", "sitemap.xml", "robots.txt"):
                    continue
            else:
                resolved = os.path.normpath(os.path.join(base, path)).replace("\\", "/")
            resolved = resolved.rstrip("/")
            if resolved not in tracked and resolved not in folders:
                broken.append(f"{doc}: {target} (no such file)")
                continue
            if frag and resolved.lower().endswith(".md") and resolved in tracked:
                if frag.lower() not in {a.lower() for a in anchors(os.path.join(ROOT, resolved))}:
                    broken.append(f"{doc}: {target} (no heading #{frag})")
    for line in broken:
        print(line)
    print(f"{len(docs)} documents checked, {len(broken)} broken links.")
    return 1 if broken else 0


if __name__ == "__main__":
    sys.exit(main())
