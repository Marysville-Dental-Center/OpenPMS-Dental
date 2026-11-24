#!/usr/bin/env python3
"""UI-12T item 5: the batch plan of the grid rollout (docs/port-notes/UI-12.md, "Tooling (UI-12T)").

    python tools/UiFit/plan_batches.py [--out docs/port-notes/UI-12-batches.tsv] [--small 20] [--small-batch 120] [--large-batch 50]

Groups the windows not yet in grid-windows into batches a slice takes at once. A window and the user controls its designer code creates
(new X() where X is another designer file's class) are one group; a user control several windows use joins them. A group of --small or
fewer designer controls (the layout snapshot's rows, the window itself not counted) goes into a batch of about --small-batch windows, a
larger one into a batch of about --large-batch. Order: the rank of fit-scan-files.tsv (the windows with the most text-fit findings first),
then the designer files the scan did not rank, by path. Every batch's files (the designer files and the window's .cs and .resx files beside
them) are its own, so two consecutive batches (and any two) share no file and can run in parallel; the plan checks it. Laid out already:
a designer file the layout snapshot gives a slice (slice_data.snapshot_owner). Nothing to lay out (UI-CLOSE): a class with a grid row of
sweep-known.tsv (nothing_to_lay_out). Deterministic; the lead reruns it after each slice merges.
Columns: batch, kind (small or large), rank, designer_file, window (namespace.class), controls, group (the group's first window).
"""
import argparse
import collections
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import fit_tools  # noqa: E402
import slice_data as sd  # noqa: E402

HEADER = "batch\tkind\trank\tdesigner_file\twindow\tcontrols\tgroup"
_new = re.compile(r"\bnew\s+(?:global::)?([\w.]+)\s*\(")
_namespace = re.compile(r"\bnamespace\s+([\w.]+)")
_class = re.compile(r"\bpartial\s+class\s+(\w+)")
_init = re.compile(r"\bvoid\s+InitializeComponent\s*\(\s*\)\s*\{(.*?)\n\t\t\}", re.S)
SHARED_USERS = 3


def read_text(root, rel):
    with open(os.path.join(root, rel), encoding="utf-8-sig", errors="replace") as f:
        return f.read()


def load(root):
    """(designer files in order with their rank, controls per file, files laid out already)."""
    data = sd.data_dir(root)
    _, _, rows = sd.read_folder(os.path.join(data, "layout-snapshot"))
    held = set(ln.split("\t")[0] for s, ln in rows if s != sd.UNSLICED)
    controls = collections.Counter()
    seen = set()
    for s, ln in rows:
        f, c = ln.split("\t")[:2]
        if s == sd.UNSLICED and f in held:
            continue
        seen.add(f)
        if c != ".":
            controls[f] += 1
    win_slices = sd.window_slices(root)
    hand = sd.read_dicts(os.path.join(data, "grid-hand"))
    owners = sd.snapshot_owners(win_slices, hand, root)
    laid = set(f for f in seen if sd.snapshot_owner(f, owners) != sd.UNSLICED)
    ranked = []
    with open(os.path.join(data, "fit-scan-files.tsv"), encoding="utf-8") as f:
        for ln in f:
            p = ln.rstrip("\r\n").split("\t")
            if len(p) > 1 and p[0].isdigit() and p[1] in seen:
                ranked.append((int(p[0]), p[1]))
    in_rank = set(f for _, f in ranked)
    order = [(r, f) for r, f in ranked] + [("", f) for f in sorted(seen) if f not in in_rank]
    nothing = nothing_to_lay_out(root)
    return [(r, f) for r, f in order if f not in laid and sd.designer_base(f) not in nothing], controls, laid


def nothing_to_lay_out(root):
    """UI-CLOSE: the class names (lower case) of the designer files with nothing to lay out that the harness cannot build (no Form or
    UserControl: the base window FormODBase, controls that draw themselves), each a row of sweep-known.tsv with rule grid and key (window)
    and its reason. They are in no batch; a window the harness builds goes on the grid's list instead (grid-windows)."""
    out = set()
    path = os.path.join(sd.data_dir(root), "sweep-known.tsv")
    if not os.path.exists(path):
        return out
    for r in sd.read_dicts(path):
        if r.get("rule") == "grid" and r.get("key") == "(window)":
            out.add(r["window"].split("@")[0].split(".")[-1].lower())
    return out


def window_name(root, rel):
    text = read_text(root, rel)
    ns = _namespace.search(text)
    cl = _class.search(text)
    name = cl.group(1) if cl else sd.designer_base(rel)
    return (ns.group(1) + "." if ns else "") + name


def own_files(root, rel):
    """The designer file and the files beside it that share its class's stem (FormX.cs, FormX.resx): a batch's files."""
    d = os.path.dirname(rel)
    stem = sd.designer_base(rel)
    out = {rel}
    try:
        for f in os.listdir(os.path.join(root, d)):
            if re.sub(r"(\.designer)?\.(cs|resx)$", "", f, flags=re.I).lower() == stem:
                out.add((d + "/" + f) if d else f)
    except OSError:
        pass
    return out


def groups(root, order):
    """Union-find over 'creates the user control' among the files still to lay out: [[files in order]] in order of their first file."""
    files = [f for _, f in order]
    by_base = collections.defaultdict(list)
    for f in files:
        by_base[sd.designer_base(f)].append(f)
    parent = {f: f for f in files}

    def find(x):
        while parent[x] != x:
            parent[x] = parent[parent[x]]
            x = parent[x]
        return x

    pos = {f: i for i, f in enumerate(files)}
    creates = {}
    users = collections.Counter()
    for f in files:
        text = read_text(root, f)
        m = _init.search(text)
        body = m.group(1) if m else text
        creates[f] = sorted(set(o for x in _new.finditer(body) for o in by_base.get(x.group(1).split(".")[-1].lower(), []) if o != f))
        for o in creates[f]:
            users[o] += 1
    for f in files:
        for other in creates[f]:
            # A library control (the shared UI controls, or one more than SHARED_USERS windows create) is its own group: it is not part
            # of one window, and joining its users would make one batch of unrelated windows.
            if "/UI/" in "/" + other or users[other] > SHARED_USERS:
                continue
            a, b = find(f), find(other)
            if a != b:
                # The root is the file first in order, so a group is named by its first window.
                if pos[a] < pos[b]:
                    parent[b] = a
                else:
                    parent[a] = b
    out = collections.OrderedDict()
    for f in files:
        out.setdefault(find(f), []).append(f)
    return list(out.values())


def plan(root, small=20, small_batch=120, large_batch=50):
    """[(batch number, kind, [files])], the rank of each file, the controls per file, the first file of each file's group."""
    order, controls, _ = load(root)
    rank = dict((f, r) for r, f in order)
    open_batch = {"small": None, "large": None}
    batches = []
    group_of = {}
    for g in groups(root, order):
        for f in g:
            group_of[f] = g[0]
        kind = "small" if sum(controls[f] for f in g) <= small else "large"
        size = small_batch if kind == "small" else large_batch
        b = open_batch[kind]
        if b is None or len(b[2]) >= size:
            b = [0, kind, []]
            batches.append(b)
            open_batch[kind] = b
        b[2].extend(g)
    for i, b in enumerate(batches):
        b[0] = i + 1
    return batches, rank, controls, group_of


def check_disjoint(root, batches):
    """Problems: a file in two batches (every pair, so consecutive batches too)."""
    owner = {}
    problems = []
    for n, _, files in batches:
        for f in files:
            for x in own_files(root, f):
                if x in owner and owner[x] != n:
                    problems.append("%s in batches %d and %d" % (x, owner[x], n))
                owner[x] = n
    return problems


def add_batch(root, plan_path, batch, slice_name):
    """Adds the windows of one batch to grid-windows/<slice>.tsv (mode tool, note 'UI-12-<slice> batch <n>'); windows already listed in any
    slice file are left out. Returns the windows of the batch, comma separated (the list the dump and the planner take)."""
    if not slice_name:
        raise SystemExit("--add needs --slice")
    rows = [r for r in sd.read_dicts(plan_path) if r["batch"] == str(batch)]
    if not rows:
        raise SystemExit("no batch %d in %s" % (batch, plan_path))
    listed = sd.window_slices(root)
    folder = os.path.join(sd.data_dir(root), "grid-windows")
    path = os.path.join(folder, slice_name + ".tsv")
    if os.path.exists(path):
        comments, header, data, nl = sd.read_file(path)
    else:
        first = sd.slice_files(folder)
        comments, header, _, nl = sd.read_file(first[0][1])
        data = []
    windows = []
    for r in rows:
        w = r["window"]
        windows.append(w)
        if w not in listed:
            data.append("\t".join([w, slice_name, "tool", "UI-12-%s batch %d" % (slice_name, batch)]))
            listed[w] = slice_name
    sd.write_file(path, comments, header, data, nl)
    return ",".join(windows)


def main():
    a = argparse.ArgumentParser()
    a.add_argument("--out", default="docs/port-notes/UI-12-batches.tsv")
    a.add_argument("--small", type=int, default=20, help="a group of this many designer controls or fewer is small")
    a.add_argument("--small-batch", type=int, default=120)
    a.add_argument("--large-batch", type=int, default=50)
    a.add_argument("--add", type=int, help="add this batch's windows (of the --out file) to Data/grid-windows/<--slice>.tsv, mode tool")
    a.add_argument("--slice", help="the slice --add adds to")
    args = a.parse_args()
    root = fit_tools.repo_root()
    if args.add:
        print(add_batch(root, os.path.join(root, args.out), args.add, args.slice))
        return
    batches, rank, controls, group_of = plan(root, args.small, args.small_batch, args.large_batch)
    problems = check_disjoint(root, batches)
    if problems:
        raise SystemExit("batches share files: " + "; ".join(problems[:10]))
    lines = []
    for n, kind, files in batches:
        for f in files:
            lines.append("\t".join([str(n), kind, str(rank[f]), f, window_name(root, f), str(controls[f]), window_name(root, group_of[f])]))
    sd.write_file(os.path.join(root, args.out), [], HEADER, lines, "\n")
    total = sum(len(b[2]) for b in batches)
    print("%d designer files in %d batches (%d small, %d large) -> %s" % (
        total, len(batches), sum(1 for b in batches if b[1] == "small"), sum(1 for b in batches if b[1] == "large"), args.out))


if __name__ == "__main__":
    main()
