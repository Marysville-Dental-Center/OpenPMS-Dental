"""UI-12T: the grid's data files as folders of one file per rollout slice (docs/port-notes/UI-12.md, "Tooling (UI-12T)").

tests/UiRefreshTests/Data/<name>/<slice>.tsv, slice = pilot, 1, 2, ... (layout-snapshot also has unsliced.tsv: the designer files no slice
owns). Every file of a folder has the same comment lines and column header; the tools and tests read the union, in slice order (pilot, then
the numbers, then other names), and a window (or a snapshot row's designer file and control) is in one file only. Two slices running at the
same time each write their own file, so their branches never edit the same data file. split_grid_data.py makes the folders.
Plain Python 3, no packages.
"""
import collections
import os
import re

import fit_tools

# folder -> the column that keys a row to its slice's file (the window), or None (the snapshot: its designer file's owner).
FOLDERS = collections.OrderedDict([
    ("grid-windows", "window"),
    ("grid-hand", "window"),
    ("grid-known", "window"),
    ("grid-runtime-texts", "window"),
    ("fit-fixes-superseded", "window"),
    ("layout-snapshot", None),
])
UNSLICED = "unsliced"
SUPERSEDED_MARK = "; superseded by the UI-12 grid (docs/port-notes/UI-12.md)"


def data_dir(root=None):
    return os.path.join(root or fit_tools.repo_root(), "tests", "UiRefreshTests", "Data")


def slice_key(name):
    """The order of slice files: pilot, then the numbered slices by number, then other names (unsliced last)."""
    if name == "pilot":
        return (0, 0, "")
    if name.isdigit():
        return (1, int(name), "")
    return (3 if name == UNSLICED else 2, 0, name)


def slice_files(folder_path):
    """[(slice, path)] of a data folder, in slice order."""
    if not os.path.isdir(folder_path):
        return []
    names = [f[:-4] for f in os.listdir(folder_path) if f.endswith(".tsv")]
    return [(n, os.path.join(folder_path, n + ".tsv")) for n in sorted(names, key=slice_key)]


def line_end(text):
    return "\r\n" if "\r\n" in text else "\n"


def read_file(path):
    """(comment lines, header, data lines, line end) of one TSV: the leading '#' lines, the column header, the rest (blank lines dropped)."""
    with open(path, encoding="utf-8", newline="") as f:
        text = f.read()
    nl = line_end(text)
    comments, header, data = [], None, []
    for ln in text.split(nl):
        ln = ln.rstrip("\r\n")
        if not ln.strip():
            continue
        if header is None and ln.startswith("#"):
            comments.append(ln)
        elif header is None:
            header = ln
        else:
            data.append(ln)
    return comments, header, data, nl


def write_file(path, comments, header, data, nl="\n"):
    """Writes a slice file (UTF-8, no BOM) only when its bytes change, so a rerun leaves every file as it is."""
    text = nl.join(list(comments) + [header] + list(data)) + nl
    if os.path.exists(path):
        with open(path, encoding="utf-8", newline="") as f:
            if f.read() == text:
                return False
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8", newline="") as f:
        f.write(text)
    return True


def read_folder(folder_path):
    """(comments, header, [(slice, line)]) of a folder: the union of its files in slice order. Every file has the first file's header."""
    comments, header, rows = [], None, []
    for s, p in slice_files(folder_path):
        c, h, data, _ = read_file(p)
        if header is None:
            comments, header = c, h
        elif h != header:
            raise ValueError("%s: header %r, not %r" % (p, h, header))
        rows.extend((s, ln) for ln in data)
    return comments, header, rows


def read_dicts(path):
    """The rows of a data file or folder as dicts (the header's columns), in slice order; comment lines skipped."""
    if os.path.isdir(path):
        _, header, rows = read_folder(path)
        lines = [ln for _, ln in rows]
    elif os.path.exists(path):
        _, header, lines, _ = read_file(path)
    else:
        return []
    if header is None:
        return []
    cols = header.split("\t")
    out = []
    for ln in lines:
        f = ln.split("\t")
        out.append(dict(zip(cols, f + [""] * (len(cols) - len(f)))))
    return out


def resolve(root, rel):
    """A data path given as the old single file (tests/.../grid-windows.tsv) or the folder: the folder when it exists."""
    p = os.path.join(root, rel)
    if p.endswith(".tsv") and not os.path.exists(p) and os.path.isdir(p[:-4]):
        return p[:-4]
    return p


def duplicates(folder_path, key_cols):
    """Keys (the values of key_cols, tab-joined) found in more than one file of the folder: [(key, [slices])]."""
    comments, header, rows = read_folder(folder_path)
    if header is None:
        return []
    idx = [header.split("\t").index(c) for c in key_cols]
    seen = collections.defaultdict(set)
    for s, ln in rows:
        f = ln.split("\t")
        seen["\t".join(f[i] for i in idx)].add(s)
    return [(k, sorted(v, key=slice_key)) for k, v in seen.items() if len(v) > 1]


def window_slices(root=None):
    """window -> slice, from grid-windows (folder, or the old single file)."""
    d = data_dir(root)
    rows = read_dicts(os.path.join(d, "grid-windows")) or read_dicts(os.path.join(d, "grid-windows.tsv"))
    out = collections.OrderedDict()
    for r in rows:
        out.setdefault(r["window"], r["slice"])
    return out


def designer_base(rel):
    """A designer file's class name: its base name without .Designer.cs (or .cs), lower case."""
    b = os.path.basename(rel)
    return re.sub(r"(\.designer)?\.cs$", "", b, flags=re.I).lower()


def snapshot_owners(win_slices, hand_rows, root=None, declared=None):
    """designer file (relative, forward slashes) or class name -> slice: a designer file belongs to the first slice (in slice order) that
    has a window of its class name (the file's base name, or since UI-12 slice 13 a class the file declares: FormCvxs in
    FormCvx.designer.cs) or a hand row in it. The C# side (SliceData.SnapshotOwners) uses the same rule. declared (rel -> class names) is
    for tests; by default the file is read under root (the repository when None); a file that is not there declares nothing."""
    by_name, by_file = {}, {}
    for w, s in sorted(win_slices.items(), key=lambda kv: slice_key(kv[1])):
        by_name.setdefault(w.split(".")[-1].lower(), s)
    for r in sorted(hand_rows, key=lambda r: slice_key(win_slices.get(r["window"], UNSLICED))):
        s = win_slices.get(r["window"])
        if s:
            by_file.setdefault(r["designer_file"], s)
    if declared is None:
        cache = {}

        def declared(rel):
            if rel not in cache:
                cache[rel] = declared_classes(os.path.join(root or fit_tools.repo_root(), rel))
            return cache[rel]
    return by_name, by_file, declared


_CLASS = re.compile(rb"\bclass\s+(\w+)")


def declared_classes(path):
    """The class names a C# file declares (lower case); none when the file is not there."""
    try:
        with open(path, "rb") as f:
            return sorted(set(m.group(1).decode("latin-1").lower() for m in _CLASS.finditer(f.read())))
    except OSError:
        return []


def snapshot_owner(rel, owners):
    by_name, by_file, declared = owners
    names = [designer_base(rel)] + [c for c in declared(rel) if c != designer_base(rel)]
    cands = [by_name.get(n) for n in names] + [by_file.get(rel)]
    cands = [s for s in cands if s]
    return min(cands, key=slice_key) if cands else UNSLICED


def hand_file_for(window, root=None):
    """The grid-hand slice file of a window (its grid-windows slice)."""
    s = window_slices(root).get(window)
    if not s:
        raise KeyError("%s is not in grid-windows: add it to its slice's file first" % window)
    return os.path.join(data_dir(root), "grid-hand", s + ".tsv")
