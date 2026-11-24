#!/usr/bin/env python3
"""UI-12T item 1: splits the grid's data files into folders of one file per rollout slice (docs/port-notes/UI-12.md, "Tooling (UI-12T)").

    python tools/UiFit/split_grid_data.py [--check]

Reads tests/UiRefreshTests/Data/{grid-windows,grid-hand,grid-known,grid-runtime-texts,layout-snapshot}.tsv when the single file exists (a
branch made before the split, merged in), otherwise the folder's union, and writes Data/<name>/<slice>.tsv: grid-windows by its slice
column, the others by their window's slice, the layout snapshot by its designer file's owner (snapshot_owner: the first slice with a window
of the file's class name or a hand row in the file; unsliced.tsv the rest). The single file is then deleted. fit-fixes.tsv stays one file:
its "; superseded by the UI-12 grid" marks move to Data/fit-fixes-superseded/<slice>.tsv (designer_file, window, control) and the note
loses the mark. Deterministic and re-runnable: a second run changes nothing, a run on the union of a folder rewrites the same bytes (and
prunes the snapshot rows that unsliced.tsv still holds for designer files a slice now owns). --check writes nothing and exits 1 when a run
would change a file.
"""
import argparse
import collections
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import fit_tools  # noqa: E402
import slice_data as sd  # noqa: E402

SNAPSHOT_COMMENTS = [
    "# The layout snapshot of the UI refresh (docs/specs/ui-refresh-plan.md section 3.7, docs/port-notes/UI-0b.md): every designer",
    "# control's Location, Size and ClientSize as InitializeComponent sets them, \".\" for the window or UserControl itself. Written by",
    "# LayoutSnapshotTests with OPMS_UI_LAYOUT_SNAPSHOT_WRITE=1; the test fails on any change that Data/fit-fixes.tsv does not list.",
    "# One file per rollout slice (UI-12T): the designer files a slice owns; unsliced.tsv the rest (a slice file wins over it).",
]
SNAPSHOT_HEADER = "file\tcontrol\tlocation\tsize\tclient_size"
SUPERSEDED_COMMENTS = [
    "# UI-12T: the rows of fit-fixes.tsv the UI-12 grid superseded (its layout replaced the fit fix), one file per rollout slice; a slice",
    "# adds its rows here instead of editing the notes of fit-fixes.tsv. Each row names a fit-fixes.tsv row (designer_file, window, control).",
]
SUPERSEDED_HEADER = "designer_file\twindow\tcontrol"


class Run:
    def __init__(self, root, check):
        self.root = root
        self.data = sd.data_dir(root)
        self.check = check
        self.changed = []

    def write(self, path, comments, header, lines, nl):
        if self.check:
            if os.path.exists(path):
                c, h, d, _ = sd.read_file(path)
                if (c, h, d) == (list(comments), header, list(lines)):
                    return
            self.changed.append(path)
            return
        if sd.write_file(path, comments, header, lines, nl):
            self.changed.append(path)

    def remove(self, path):
        if os.path.exists(path):
            self.changed.append(path + " (removed)")
            if not self.check:
                os.remove(path)

    def source(self, name):
        """(comments, header, [(slice or None, line)], line end, single file path or None)."""
        single = os.path.join(self.data, name + ".tsv")
        if os.path.exists(single):
            c, h, d, nl = sd.read_file(single)
            return c, h, [(None, ln) for ln in d], nl, single
        c, h, rows = sd.read_folder(os.path.join(self.data, name))
        nl = "\n"
        files = sd.slice_files(os.path.join(self.data, name))
        if files:
            nl = sd.read_file(files[0][1])[3]
        return c, h, rows, nl, None

    def write_folder(self, name, comments, header, by_slice, nl, single):
        folder = os.path.join(self.data, name)
        for s in sorted(by_slice, key=sd.slice_key):
            self.write(os.path.join(folder, s + ".tsv"), comments, header, by_slice[s], nl)
        for s, p in sd.slice_files(folder):
            if s not in by_slice:
                self.remove(p)
        if single:
            self.remove(single)

    def split_windows(self):
        c, h, rows, nl, single = self.source("grid-windows")
        if h is None:
            raise SystemExit("no grid-windows data")
        by_slice = collections.OrderedDict()
        seen = set()
        for _, ln in rows:
            f = ln.split("\t")
            if f[0] in seen:
                raise SystemExit("grid-windows: %s is listed twice" % f[0])
            seen.add(f[0])
            by_slice.setdefault(f[1], []).append(ln)
        self.write_folder("grid-windows", c, h, by_slice, nl, single)
        return collections.OrderedDict((ln.split("\t")[0], ln.split("\t")[1]) for _, ln in rows)

    def split_by_window(self, name, win_slices):
        c, h, rows, nl, single = self.source(name)
        if h is None:
            return []
        col = h.split("\t").index("window")
        by_slice = collections.OrderedDict()
        out = []
        for _, ln in rows:
            w = ln.split("\t")[col]
            s = win_slices.get(w)
            if s is None:
                raise SystemExit("%s: %s is not in grid-windows: %s" % (name, w, ln[:120]))
            by_slice.setdefault(s, []).append(ln)
            out.append(dict(zip(h.split("\t"), ln.split("\t"))))
        self.write_folder(name, c, h, by_slice, nl, single)
        return out

    def split_snapshot(self, owners):
        c, h, rows, nl, single = self.source("layout-snapshot")
        if h is None:
            return
        if single is None:
            # The union: a slice file wins over unsliced.tsv for a designer file it holds.
            held = set(ln.split("\t")[0] for s, ln in rows if s != sd.UNSLICED)
            rows = [(s, ln) for s, ln in rows if s != sd.UNSLICED or ln.split("\t")[0] not in held]
        blocks = collections.OrderedDict()
        for _, ln in rows:
            blocks.setdefault(ln.split("\t")[0], []).append(ln)
        by_slice = collections.OrderedDict()
        for f in sorted(blocks):
            by_slice.setdefault(sd.snapshot_owner(f, owners), []).extend(blocks[f])
        self.write_folder("layout-snapshot", SNAPSHOT_COMMENTS, SNAPSHOT_HEADER, by_slice, nl, single)

    def split_superseded(self, win_slices, owners):
        path = os.path.join(self.data, "fit-fixes.tsv")
        with open(path, encoding="utf-8", newline="") as f:
            text = f.read()
        nl = sd.line_end(text)
        lines = text.split(nl)
        header = None
        keys = []
        for i, ln in enumerate(lines):
            if header is None:
                if ln.startswith("designer_file\t"):
                    header = ln.split("\t")
                continue
            if not ln.strip():
                continue
            f = ln.split("\t")
            keys.append("\t".join(f[:3]))
            if f[-1].endswith(sd.SUPERSEDED_MARK):
                f[-1] = f[-1][:-len(sd.SUPERSEDED_MARK)]
                lines[i] = "\t".join(f)
                self.superseded.add("\t".join(f[:3]))
        folder = os.path.join(self.data, "fit-fixes-superseded")
        _, _, rows = sd.read_folder(folder)
        for _, ln in rows:
            self.superseded.add(ln)
        order = {k: i for i, k in enumerate(keys)}
        missing = [k for k in self.superseded if k not in order]
        if missing:
            raise SystemExit("fit-fixes-superseded names rows fit-fixes.tsv does not have: " + "; ".join(sorted(missing)[:5]))
        by_slice = collections.OrderedDict()
        for k in sorted(self.superseded, key=lambda k: order[k]):
            designer, window, _ = k.split("\t")
            s = win_slices.get(window) or sd.snapshot_owner(designer, owners)
            by_slice.setdefault(s, []).append(k)
        new_text = nl.join(lines)
        if new_text != text:
            self.changed.append(path)
            if not self.check:
                with open(path, "w", encoding="utf-8", newline="") as f:
                    f.write(new_text)
        self.write_folder("fit-fixes-superseded", SUPERSEDED_COMMENTS, SUPERSEDED_HEADER, by_slice, nl, None)

    def run(self):
        self.superseded = set()
        win_slices = self.split_windows()
        hand = self.split_by_window("grid-hand", win_slices)
        self.split_by_window("grid-known", win_slices)
        self.split_by_window("grid-runtime-texts", win_slices)
        owners = sd.snapshot_owners(win_slices, hand, self.root)
        self.split_snapshot(owners)
        self.split_superseded(win_slices, owners)
        for name, key in sd.FOLDERS.items():
            cols = [key] if key else ["file", "control"]
            dup = sd.duplicates(os.path.join(self.data, name), cols) if not self.check else []
            dup = [d for d in dup if not (name == "layout-snapshot" and sd.UNSLICED in d[1])]
            if dup:
                raise SystemExit("%s: in two files: %s" % (name, dup[:5]))


def main():
    a = argparse.ArgumentParser()
    a.add_argument("--check", action="store_true", help="write nothing; exit 1 when a run would change a file")
    a.add_argument("--root", help="the repository (default: this checkout)")
    args = a.parse_args()
    r = Run(args.root or fit_tools.repo_root(), args.check)
    r.run()
    for p in r.changed:
        print(("would change " if args.check else "changed ") + os.path.relpath(p, r.root).replace("\\", "/"))
    print("%d files %s" % (len(r.changed), "to change" if args.check else "changed"))
    if args.check and r.changed:
        sys.exit(1)


if __name__ == "__main__":
    main()
