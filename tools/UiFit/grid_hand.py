"""UI-12 round 3: the grid's rules applied by a layout script, for the parts of windows the planner cannot place (docs/port-notes/UI-12.md,
section 2). A window's script (tools/UiFit/hand/<Window>.py, a function layout(h)) names its rows and columns; this module places them with the
numbers of grid_tools.GRID (the field height measured from the window's tree, the row pitch, the label gap, the padding, the title band) and
writes the result as hand rows (tests/UiRefreshTests/Data/grid-hand.tsv, rule "by hand (layout script): ..."), which the applier writes into
the designer file like any hand row. The script reads the window's tree as main builds it (the dump of the branch point), so a run gives the
same rows every time and applying them twice changes nothing.

    python tools/UiFit/grid_hand.py --tree <tree-before.tsv> --window FormInsPlan [--check]

replaces the window's script rows in grid-hand.tsv (rows of other windows and the window's other hand rows stay) and prints what its own check
finds in the parts it placed: siblings that overlap (by their text for labels and check boxes), controls past their container, labels that do
not fit their text.
"""
import argparse
import collections
import importlib.util
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import fit_tools  # noqa: E402
import grid_tools as g  # noqa: E402

GRID = g.GRID
RULE = "by hand (layout script): "


def read_runtime_texts():
    """(window, control) -> [width]: the runtime texts of tests/UiRefreshTests/Data/grid-runtime-texts.tsv."""
    out = collections.defaultdict(list)
    p = os.path.join(fit_tools.repo_root(), "tests", "UiRefreshTests", "Data", "grid-runtime-texts.tsv")
    if not os.path.exists(p):
        return out
    with open(p, encoding="utf-8") as f:
        for ln in f:
            ln = ln.rstrip("\r\n")
            if not ln or ln.startswith("#") or ln.startswith("window\t"):
                continue
            r = ln.split("\t")
            out[(r[0], r[1])].append(int(r[3]))
    return out


RUNTIME = read_runtime_texts()


class Hand:
    def __init__(self, window, tree, designer):
        self.window = window
        self.tree = tree
        self.designer = designer
        # The window's clinic pickers, as the planner reads them (their label areas: clinic_w).
        g.NO_LABEL_PICKERS.clear()
        g.NO_LABEL_PICKERS.update(getattr(designer, "no_label", ()))
        g.MULTI_PICKERS.clear()
        g.MULTI_PICKERS.update(getattr(designer, "multi_pickers", ()))
        g.MULTI_PICKERS.update(g.multi_pickers_in_code(fit_tools.repo_root(), window.split(".")[-1]))
        self.by_name = {}
        for n in g.walk(tree):
            if n is not tree and n.name:
                self.by_name.setdefault(n.name, n)
        p = g.Planner(fit_tools.repo_root())
        p.measure(tree)
        self.H = p.field_height
        self.CH = p.check_height
        self.touched = collections.OrderedDict()  # id -> node, the controls the script placed
        self.sized = set()  # the controls the script gave a size
        self.why = {}
        self.containers = []

    # -- nodes ---------------------------------------------------------------------------------------------------------------------------
    def n(self, name):
        if name == ".":
            return self.tree
        if name not in self.by_name:
            raise KeyError("%s: no control %s" % (self.window, name))
        return self.by_name[name]

    def set(self, name, x=None, y=None, w=None, h=None, why=None):
        n = self.n(name)
        if x is not None:
            n.nx = int(x)
        if y is not None:
            n.ny = int(y)
        if w is not None:
            n.nw = int(w)
            self.sized.add(id(n))
        if h is not None:
            n.nh = int(h)
            self.sized.add(id(n))
        self.touched[id(n)] = n
        if why:
            self.why[id(n)] = why
        return n

    def text_align(self, name, value):
        n = self.n(name)
        if n.text_align != value:
            n.new_text_align = value
        # Slice 1 fix round 9: a right-to-left label mirrors its TextAlign (MiddleRight draws at the left): an aligned label is not right to
        # left (grid_tools does the same for the labels it aligns).
        if n.name in self.designer.rtl and n.kind == "label":
            n.rtl = True
        self.touched[id(n)] = n

    def check_left(self, name):
        n = self.n(name)
        if n.check_align and n.check_align != "MiddleLeft" and n.check_align != "TopLeft":
            n.new_check_align = "MiddleLeft"
        if n.name in self.designer.rtl:
            n.rtl = True
        self.touched[id(n)] = n

    def lw(self, name):
        """The width a label needs for its text on one line (snapped up to the grid): the widest of its designer text and the texts the code
        gives it at run time (tests/UiRefreshTests/Data/grid-runtime-texts.tsv, slice 1 fix round 8)."""
        n = self.n(name)
        tw = max([n.text_w or 0] + self.runtime_widths(name))
        return g.snap_up(tw + 4) if tw else n.w

    def runtime_widths(self, name):
        """The widths (px on one line at Segoe UI 9) of the texts the window's code gives the control (grid-runtime-texts.tsv)."""
        return [w for (win, ctl), ws in RUNTIME.items() if win == self.window and ctl == name for w in ws]

    def cw(self, name):
        n = self.n(name)
        return g.check_width(n) if n.text_w else 16

    def clinic_w(self, name):
        """A clinic picker's label area: its box starts this far into its bounds (grid_tools.clinic_label_w)."""
        return g.clinic_label_w(self.n(name))

    def group_rows(self, gname, x, y, w, button=None, align=None, hidden_first=None):
        """UI-12 slice 1: lays out a group box of label + field rows in their drawn order: one field column, a picker button (24 px) on the
        first row at the content edge before the labels, the labels right-aligned 6 px before the column, the fields to the content edge.
        align: the field column's x in the group (to line up with another group); hidden_first: a control hidden in the designer that stands
        at the content edge of the first row. Returns the Box and the field column's x."""
        H = self.H
        g = self.n(gname)
        kids = [k for k in g.children if k.visible or k.name == hidden_first]
        labels = sorted([k for k in kids if k.kind == "label"], key=lambda k: k.y)
        fields = sorted([k for k in kids if k.kind == "field"], key=lambda k: k.y)
        lw = max(self.lw(k.name) for k in labels)
        b = self.box(gname, x, y, w)
        F = align if align is not None else b.left + (24 + 6 if button else 0) + lw + 6
        for i, (lb, fd) in enumerate(zip(labels, fields)):
            yy = b.y
            b.field(lb.name, fd.name, F, "R", lw=lw)
            if i == 0 and button:
                self.set(button, b.left, yy, 24, H)
            if i == 0 and hidden_first:
                self.check_left(hidden_first)
                self.set(hidden_first, b.left, yy + (H - self.CH) // 2, self.cw(hidden_first), self.CH)
        return b, F

    def caption_row(self, gname, cols, x, y, w, button=None, under=()):
        """UI-12 slice 2: a group box of one row of captioned fields (the merge windows): each column a caption label over its field (the
        caption 2 px over the field, both from the column's x), the columns 6 px apart from the content edge, the field keeping its width
        (a column is as wide as the wider of its field and its caption's text); a check box as a field stands under the start of its caption;
        button: a button on the field row at the content's right edge; under: controls placed one per row under the field row, from the
        content edge. The group is laid at x, y, w and as tall as its content + 8. Returns the group's height."""
        H = self.H
        grp = self.n(gname)
        top = (grp.title_bottom or 18) + GRID["title_gap"]
        cap_h = 17
        yf = top + cap_h + GRID["caption_gap"]
        cx = GRID["pad"]
        for cap, fld in cols:
            c = self.n(cap)
            f = self.n(fld)
            cw = max(f.nw, self.lw(cap))
            self.set(cap, cx, top, self.lw(cap), cap_h)
            self.text_align(cap, "BottomLeft")
            if f.kind in ("check", "radio"):
                self.set(fld, cx + 2, yf + (H - f.nh) // 2)
            else:
                self.set(fld, cx, yf, f.nw, H)
            cx += cw + GRID["item_gap"]
        if button:
            b = self.n(button)
            self.set(button, w - GRID["pad"] - b.nw, yf, b.nw, H)
        yb = yf + H
        for name in under:
            k = self.n(name)
            hh = self.CH if k.kind in ("check", "radio") else H
            if k.kind in ("check", "radio"):
                self.check_left(name)
                self.set(name, GRID["pad"], yb + GRID["row_gap"], self.cw(name), hh)
            else:
                self.set(name, GRID["pad"], yb + GRID["row_gap"], None, hh)
            yb += GRID["row_gap"] + hh
        gh = yb + GRID["pad"]
        self.set(gname, x, y, w, gh)
        self.containers.append(Box(self, gname, None, None, None, None, None, None, None))
        return gh

    def frame_left(self):
        """UI-12 slice 1: the window's left content edge at the padding (8). The planner keeps a window's left column at main's x when the
        controls there are anchored (12, 13, 34, 45 in the slice); this moves every control of the window's left edge to 8, and a wide one
        (120 px or more, not a button) keeps its right edge (it grows by as much). Returns the shift."""
        kids = [k for k in self.tree.children if k.dock in ("None", "") and k.w > 0 and k.h > 0]
        shown = [k for k in kids if k.visible]
        if not shown:
            return 0
        minx = min(k.nx for k in shown)
        dx = minx - GRID["pad"]
        if dx <= 0:
            return 0
        for k in kids:
            if k.nx != minx:
                continue
            if k.nw >= 120 and k.kind != "button":
                self.set(k.name, GRID["pad"], None, k.nw + dx, None)
            else:
                self.set(k.name, GRID["pad"], None)
        return dx

    def watch(self, *names):
        """Containers the script laid out without a Box: the check reads them too."""
        for name in names:
            self.containers.append(Box(self, name, None, None, None, None, None, None, None))

    def box(self, name, x=None, y=None, w=None, h=None, pad=None, top=None, left=None):
        b = Box(self, name, x, y, w, h, pad, top, left)
        self.containers.append(b)
        return b

    # -- output --------------------------------------------------------------------------------------------------------------------------
    def rows(self):
        out = []
        for n in self.touched.values():
            why = RULE + self.why.get(id(n), "on the grid")
            if n is self.tree:
                if (n.nw, n.nh) != (n.w, n.h):
                    prop = "ClientSize" if self.designer.size(".", "ClientSize") else "Size"
                    # The client panel of a FormODBase and a module's root are the designer's client size (the tree may be a planned one).
                    sz = self.designer.size(".", prop)
                    out.append((".", prop, "%d,%d" % (sz if sz else (n.w, n.h)), "%d,%d" % (n.nw, n.nh), why))
                continue
            # The tree's coordinates are the designer's (the controls of a FormODBase's client panel and of a module); the old value is
            # the designer's, which may differ where the tree is a planned one (a part the planner laid out first).
            loc = self.designer.point(n.name)
            sz = self.designer.size(n.name)
            if (n.nx, n.ny) != (n.x, n.y) or (loc and loc != (n.nx, n.ny)):
                out.append((n.name, "Location", "%d,%d" % loc if loc else "(none)", "%d,%d" % (n.nx, n.ny), why))
            if (n.nw, n.nh) != (n.w, n.h) or (sz and sz != (n.nw, n.nh) and id(n) in self.sized):
                out.append((n.name, "Size", "%d,%d" % sz if sz else "(none)", "%d,%d" % (n.nw, n.nh), why))
                if n.autosize and n.kind in ("label", "check", "radio") and n.type != "OpenPmsDental.UI.CheckBox":
                    out.append((n.name, "AutoSize", "true", "false", why))
            if n.new_text_align:
                out.append((n.name, "TextAlign", n.text_align, n.new_text_align, why))
            if n.new_check_align:
                out.append((n.name, "CheckAlign", n.check_align, n.new_check_align, RULE + "the box on the left of its text"))
            if n.rtl and n.kind == "label":
                out.append((n.name, "RightToLeft", "Yes", "No", RULE + "aligned, not right to left (which mirrors it)"))
            elif n.rtl:
                out.append((n.name, "RightToLeft", "Yes", "No", RULE + "the box on the left of its text (not right to left)"))
        return out

    def check(self):
        """The script's own check of the containers it laid out: overlaps by text, controls past the container, labels too narrow."""
        problems = []
        for b in self.containers:
            c = b.node
            kids = [k for k in c.children if k.dock in ("None", "") and k.visible and k.w > 0 and k.h > 0]
            for i, a in enumerate(kids):
                ax, aw = g.ink_x_new(a)
                for k in kids[i + 1:]:
                    bx, bw = g.ink_x_new(k)
                    ix = min(ax + aw, bx + bw) - max(ax, bx)
                    iy = min(a.ny + a.nh, k.ny + k.nh) - max(a.ny, k.ny)
                    if ix > 2 and iy > 2 and g.overlap_ratio(a, k) < 0.6:
                        problems.append("%s: overlap %s %s (%dx%d)" % (c.name or ".", a.name, k.name, ix, iy))
                if c.kind != "tabs" and (a.nx < 0 or a.ny < 0 or a.nx + a.nw > c.nw or a.ny + a.nh > c.nh):
                    problems.append("%s: %s past the edge (%d,%d %dx%d in %dx%d)" % (c.name or ".", a.name, a.nx, a.ny, a.nw, a.nh, c.nw, c.nh))
                if a.kind == "label" and a.text_w and a.text_w + 2 > a.nw and a.nh < 1.6 * (a.text_h or 16):
                    problems.append("%s: label %s %d px for a %d px text" % (c.name or ".", a.name, a.nw, a.text_w))
        return problems


class Box:
    """One container's content: rows top to bottom from its padding (a group box's content starts below its title band)."""

    def __init__(self, hand, name, x, y, w, h, pad, top, left):
        self.h = hand
        self.node = hand.n(name)
        n = self.node
        if any(v is not None for v in (x, y, w, h)):
            hand.set(name, x, y, w, h)
        self.pad = GRID["pad"] if pad is None else pad
        if top is not None:
            self.top = top
        elif n.kind == "group" and n.title_bottom:
            self.top = n.title_bottom + GRID["title_gap"]
        else:
            self.top = self.pad
        self.left = self.pad if left is None else left
        self.y = self.top
        self.last_gap = 0
        self.cursor = None
        self.bottom_used = self.top

    @property
    def right(self):
        return self.node.nw - self.pad

    def width(self, w, x):
        if w == "R":
            return self.right - x
        if isinstance(w, tuple):  # ("R", k): to the right edge less k
            return self.right - w[1] - x
        return w

    def item_h(self, n, h=None):
        if h is not None:
            return h
        k = n.kind
        if k == "field":
            # The clinic picker at the field height like every combo box (fix round); the date picker lays its own box out.
            if "ODDatePicker" in n.type:
                return n.nh
            return self.h.H
        if k == "button" and n.h <= self.h.H + 6:
            return self.h.H
        if k in ("check", "radio"):
            return self.h.CH
        if k == "label" and n.h <= self.h.H + 6:
            return self.h.H
        return n.nh

    def row(self, *cells, gap=None, h=None):
        """cells: (name, x, w[, h]) with x None (6 px after the cell before), a number, or ("F", label name) for a label + field pair (the label
        right-aligned 6 px before x); w a number, None (keep), "R" (to the content's right edge) or ("R", k)."""
        placed = []
        cursor = None
        for cell in cells:
            name, x, w = cell[0], cell[1], cell[2]
            ih = cell[3] if len(cell) > 3 else None
            n = self.h.n(name)
            if x is None:
                x = cursor + GRID["item_gap"]
            if n.kind in ("check", "radio"):
                self.h.check_left(name)
                ww = self.width(w, x) if w is not None else (self.h.cw(name) if n.text_w else n.nw)
            else:
                ww = self.width(w, x) if w is not None else n.nw
            hh = self.item_h(n, ih)
            placed.append((n, x, ww, hh))
            cursor = x + ww
        rh = h if h is not None else max(p[3] for p in placed)
        for n, x, ww, hh in placed:
            yy = self.y + (rh - hh) // 2 if hh <= rh else self.y
            self.h.set(n.name, x, yy, ww, hh)
            if n.kind == "label" and not (n.text_align or "").startswith("Top") and hh <= self.h.H and n.text_align not in ("MiddleLeft", "MiddleRight", "MiddleCenter"):
                self.h.text_align(n.name, "MiddleLeft")
        self.bottom_used = max(self.bottom_used, self.y + rh)
        self.y += rh + (GRID["row_gap"] if gap is None else gap)
        self.last_gap = GRID["row_gap"] if gap is None else gap
        self.cursor = cursor
        return placed

    def field(self, label, field, x, w=None, after=(), gap=None, lw=None):
        """A label + field row: the label right-aligned 6 px before x, the field at x, then the cells of after (name, x or None, w)."""
        n = self.h.n(label)
        lwid = lw if lw is not None else self.h.lw(label)
        cells = [(label, x - GRID["label_gap"] - lwid, lwid), (field, x, w)] + list(after)
        out = self.row(*cells, gap=gap)
        if n.text_align != "MiddleRight":
            self.h.text_align(label, "MiddleRight")
        return out

    def checks(self, names, x, gap=None, after=None, h=None):
        """Check boxes (or radio buttons) one per row at x, the list pitch apart. after: name -> cells that follow the check box's text. h: one
        row height for every row (a list where some rows hold a button keeps one pitch)."""
        for name in names:
            cells = [(name, x, None)]
            if after and name in after:
                cells += list(after[name])
            self.row(*cells, gap=GRID["list_gap"] if gap is None else gap, h=h)

    def place(self, name, x=None, w=None, h=None, gap=None):
        """A block (a group box, a list, a grid) at the cursor's row: x (default the content's left), w ("R": to the right edge)."""
        x = self.left if x is None else x
        n = self.h.n(name)
        ww = self.width(w, x) if w is not None else n.nw
        hh = n.nh if h is None else h
        self.h.set(name, x, self.y, ww, hh)
        self.bottom_used = max(self.bottom_used, self.y + hh)
        g_ = GRID["sibling_gap"] if gap is None else gap
        self.y += hh + g_
        self.last_gap = g_
        return n

    def skip(self, dy):
        self.y += dy

    def end(self, w=None, h=None):
        """The container's size: its width as given (or kept), its height from its last row and the bottom padding."""
        n = self.node
        hh = self.bottom_used + self.pad if h is None else h
        self.h.set(n.name if n is not self.h.tree else ".", None, None, w, hh)
        return hh


def load_script(window_simple):
    p = os.path.join(os.path.dirname(os.path.abspath(__file__)), "hand", window_simple + ".py")
    spec = importlib.util.spec_from_file_location("hand_" + window_simple, p)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


def run(tree_path, window_simple):
    root = fit_tools.repo_root()
    window = "OpenPmsDental." + window_simple
    trees = g.load_tree(tree_path, {window})
    tree = trees[window]
    wf = fit_tools.window_files(root, [window])
    designer = g.Designer(root, wf[window][0])
    for n in g.walk(tree):
        if n is not tree and n.name in designer.name_to_field and n.name not in designer.declared:
            n.name = designer.name_to_field[n.name]
    hand = Hand(window, tree, designer)
    mod = load_script(window_simple)
    mod.layout(hand)
    return hand, designer


def main():
    a = argparse.ArgumentParser()
    a.add_argument("--tree", required=True)
    a.add_argument("--window", required=True, help="the window's class name, e.g. FormInsPlan")
    a.add_argument("--hand-file", default="tests/UiRefreshTests/Data/grid-hand.tsv")
    a.add_argument("--tree-out", help="write the laid-out tree (GridDump format) for render.py")
    a.add_argument("--dry", action="store_true", help="print the rows, do not write the hand file")
    args = a.parse_args()
    hand, designer = run(args.tree, args.window)
    rows = hand.rows()
    window = "OpenPmsDental." + args.window
    for p in hand.check():
        print("check:", p)
    if args.tree_out:
        g.write_tree(args.tree_out, {window: hand.tree})
    if args.dry:
        for r in rows:
            print("\t".join(r))
        return
    path = os.path.join(fit_tools.repo_root(), args.hand_file)
    with open(path, "r", encoding="utf-8", newline="") as f:
        text = f.read()
    nl = "\r\n" if "\r\n" in text else "\n"
    lines = text.split(nl)
    keep = [ln for ln in lines if ln and not (ln.split("\t")[1:2] == [window] and "\t" + RULE in ln)]
    keep += ["\t".join((designer.rel, window) + r) for r in rows]
    with open(path, "w", encoding="utf-8", newline="") as f:
        f.write(nl.join(keep) + nl)
    print("%s: %d rows" % (window, len(rows)))


if __name__ == "__main__":
    main()
