#!/usr/bin/env python3
# UI-12 layout grid tools (docs/port-notes/UI-12.md): the planner reads the control tree of each grid window (the opt-in GridDump test of
# tests/UiRefreshTests, built off screen at the program's font) with the window's designer and code files, finds rows and columns, applies
# the grid's rules (section 1 of the note; the numbers are GRID below) and writes a plan TSV (designer file, window, control, property, old,
# new, rule); the applier writes a plan into the designer files as bytes (latin-1 round trip), changing only the digits of a Location or Size
# line, one alignment word, or inserting one line with its neighbour's line end, so every line end and the BOM stay. Deterministic: a plan made
# from the tree of the applied windows is empty. Plain Python 3, no packages.
#
#   grid_tools.py plan  --tree <grid-tree.tsv> [--windows-file tests/UiRefreshTests/Data/grid-windows.tsv] [--windows A,B] --out <plan.tsv>
#                       [--unplaced <tsv>]
#   grid_tools.py apply --plan <plan.tsv>            (also the hand plan, tests/UiRefreshTests/Data/grid-hand.tsv)
import argparse
import collections
import csv
import math
import os
import re
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import fit_tools  # noqa: E402

csv.field_size_limit(1 << 24)

# The grid, in px at 96 dpi for Segoe UI 9 pt (UI-12.md section 1 and 1.1).
GRID = {
    "pad": 8,                 # container padding on every side
    "title_gap": 4,           # a group box's content starts this far below its title band (measured: 8 px below the ink, section 1.1)
    "label_gap": 6,           # a label sits this far before its field
    "row_gap": 6,             # between field rows
    "list_gap": 4,            # between consecutive check boxes or radio buttons
    "caption_gap": 2,         # a caption label above the control it names
    "section_gap": 12,        # rows that were clearly apart (a gap of section_from or more) stay apart by this
    "section_from": 14,
    "sibling_gap": 8,         # between containers side by side or stacked
    "item_gap": 6,            # between items of one row (a field and its button or its unit label)
    "band_gap": 16,           # between two label + field bands of one row
    "edge_snap": 16,          # right edges of one column within this share one edge
    "column_snap": 16,        # left edges (anchors) within this are one column
    "field_column_snap": 40,  # ... and for the fields of label + field rows (a label column right-aligned to them)
    "span_snap": 32,          # a container within this of its parent's content edges spans the content
    "snap": 4,                # widths snap to this
    "field_height": 23,       # a single-line field; replaced by the measured height of a text box at the program's font
    "check_height": 18,       # a check box or radio button row (UI.CheckBox's own height); at least the text line + 2
    "check_box": 13,          # UI.CheckBox's box (12 + the 1 px line)
    "check_text": 3,          # between the box and the text
    "dialog_gap": 8,          # between dialog buttons
}

LABEL_TYPES = ("System.Windows.Forms.Label", "System.Windows.Forms.LinkLabel")
CHECK_TYPES = ("OpenPmsDental.UI.CheckBox", "System.Windows.Forms.CheckBox")
RADIO_TYPES = ("System.Windows.Forms.RadioButton",)
BUTTON_TYPES = ("OpenPmsDental.UI.Button", "System.Windows.Forms.Button")
GROUP_TYPES = ("OpenPmsDental.UI.GroupBox", "System.Windows.Forms.GroupBox")
PANEL_TYPES = ("System.Windows.Forms.Panel", "OpenPmsDental.UI.PanelOD", "OpenPmsDental.UI.PanelBorders")
TAB_TYPES = ("OpenPmsDental.UI.TabControl", "System.Windows.Forms.TabControl")
TABPAGE_TYPES = ("OpenPmsDental.UI.TabPage", "System.Windows.Forms.TabPage")
SINGLE_LINE_HINTS = ("TextBox", "Valid", "ComboBox", "DateTimePicker", "UpDown", "ODDatePicker")
DIALOG_NAMES = ("butOK", "butSave", "butCancel", "butClose")
# Owner-drawn surfaces and their windows' regions that the grid never moves (section 1, "Untouched").
OWNER_DRAWN_HINTS = ("ToothChart", "ContrApptPanel", "ControlApptPanel", "SheetEditMobile", "SheetFill", "EFormFill", "ClaimForm", "PrintPreview",
                     "GraphicsToothChart", "ToothChartWrapper", "ToothChartRelay")


def snap(v, step=None):
    step = step or GRID["snap"]
    return int(round(v / float(step))) * step


def snap_up(v, step=None):
    step = step or GRID["snap"]
    return int(math.ceil(v / float(step))) * step


def unclean(s):
    out = []
    i = 0
    while i < len(s):
        c = s[i]
        if c == "\\" and i + 1 < len(s):
            n = s[i + 1]
            out.append({"t": "\t", "n": "\n", "\\": "\\"}.get(n, n))
            i += 2
            continue
        out.append(c)
        i += 1
    return "".join(out)


class Node:
    __slots__ = ("window", "path", "name", "type", "index", "x", "y", "w", "h", "cx", "cy", "cw", "ch", "visible", "dock", "anchor", "autosize",
                 "sizes_itself", "in_layout_panel", "check_align", "text_align", "multiline", "font_h", "text_w", "text_h", "title_bottom", "pref_h",
                 "text", "children", "parent", "nx", "ny", "nw", "nh", "editable", "fixed", "why_fixed", "new_check_align", "new_text_align",
                 "autosize_off", "rule", "members", "spanned")

    def __init__(self, r):
        self.window = r["window"]
        self.path = unclean(r["path"])
        self.name = unclean(r["name"])
        self.type = r["type"]
        self.index = int(r["index"])
        self.x, self.y, self.w, self.h = int(r["x"]), int(r["y"]), int(r["width"]), int(r["height"])
        self.cx, self.cy = int(r["client_x"]), int(r["client_y"])
        self.cw, self.ch = int(r["client_width"]), int(r["client_height"])
        self.visible = r["visible"] == "1"
        self.dock = r["dock"]
        self.anchor = int(r["anchor"])
        self.autosize = r["auto_size"] == "1"
        self.sizes_itself = r["sizes_itself"] == "1"
        self.in_layout_panel = r["layout_panel_child"] == "1"
        self.check_align = r["check_align"]
        self.text_align = r["text_align"]
        self.multiline = r["multiline"] == "1"
        self.font_h = int(r["font_height"])
        self.text_w = int(r["text_width"])
        self.text_h = int(r["text_height"])
        self.title_bottom = int(r["title_bottom"])
        self.pref_h = int(r["preferred_height"])
        self.text = unclean(r["text"])
        self.children = []
        self.parent = None
        self.nx, self.ny, self.nw, self.nh = self.x, self.y, self.w, self.h
        self.editable = False
        self.fixed = False
        self.why_fixed = ""
        self.new_check_align = None
        self.new_text_align = None
        self.autosize_off = False
        self.rule = collections.OrderedDict()
        self.members = None
        self.spanned = False

    # anchor bits: Top 1, Bottom 2, Left 4, Right 8
    @property
    def anchored_bottom_only(self):
        return bool(self.anchor & 2) and not (self.anchor & 1)

    @property
    def anchored_right_only(self):
        return bool(self.anchor & 8) and not (self.anchor & 4)

    def is_(self, types):
        return self.type in types

    @property
    def kind(self):
        t = self.type
        if t in LABEL_TYPES:
            return "label"
        if t in CHECK_TYPES:
            return "check"
        if t in RADIO_TYPES:
            return "radio"
        if t in BUTTON_TYPES:
            return "button"
        if t in GROUP_TYPES:
            return "group"
        if t in TAB_TYPES:
            return "tabs"
        if t in TABPAGE_TYPES:
            return "tabpage"
        short = t.split(".")[-1]
        if any(h in short for h in SINGLE_LINE_HINTS) and not self.multiline and self.h <= 30:
            return "field"
        if t in PANEL_TYPES:
            return "panel"
        return "other"

    def add_rule(self, prop, rule):
        self.rule.setdefault(prop, rule)


def load_tree(path, windows=None):
    rows = fit_tools.read_tsv(path)
    by_window = collections.OrderedDict()
    for r in rows:
        if windows and r["window"] not in windows:
            continue
        by_window.setdefault(r["window"], []).append(Node(r))
    trees = collections.OrderedDict()
    for w, nodes in by_window.items():
        by_path = {}
        root = None
        for n in nodes:
            by_path[n.path] = n
            if n.path == ".":
                root = n
                continue
            pp = n.path.rsplit("/", 1)[0] if "/" in n.path else "."
            p = by_path.get(pp)
            if p is not None:
                n.parent = p
                p.children.append(n)
        trees[w] = root
    return trees


# ---------------------------------------------------------------------------------------------------------------------------------------
# The designer and code files

class Designer:
    def __init__(self, root, rel):
        self.rel = rel
        self.path = os.path.join(root, rel)
        with open(self.path, "rb") as f:
            self.text = f.read().decode("latin-1")
        self.declared = {m.group(1): m.group(2) for m in re.finditer(r"this\.(\w+) ?= ?new ([\w.]+)\(", self.text)}
        self.docked = {m.group(1): m.group(2) for m in re.finditer(r"this\.(\w+)\.Dock ?= ?System\.Windows\.Forms\.DockStyle\.(\w+);", self.text)}
        self.adds = {}
        for m in re.finditer(r"this\.(?:(\w+)\.)?Controls\.Add\(this\.(\w+)\)", self.text):
            self.adds[m.group(2)] = m.group(1) or "."

    def point(self, name, prop="Location"):
        m = re.search(r"this\.%s\.%s ?= ?new System\.Drawing\.Point\((-?\d+), ?(-?\d+)\)" % (re.escape(name), prop), self.text)
        return (int(m.group(1)), int(m.group(2))) if m else None

    def size(self, name, prop="Size"):
        target = "this." + name + "." + prop if name != "." else "this." + prop
        m = re.search(r"%s ?= ?new System\.Drawing\.Size\((\d+), ?(\d+)\)" % re.escape(target), self.text)
        return (int(m.group(1)), int(m.group(2))) if m else None


def code_files_for(root, simple):
    """The .cs files (not designer files) that declare (partial) class simple."""
    try:
        out = subprocess.check_output(["git", "grep", "-l", "-E", r"partial class %s\b|class %s\b" % (simple, simple), "--", "*.cs"],
                                      cwd=root).decode("utf-8")
    except subprocess.CalledProcessError:
        return []
    return [x for x in out.splitlines() if x and not re.search(r"\.designer\.cs$", x, re.I)]


def runtime_positioned(root, simple, names):
    """Names the window's code positions or sizes at run time: name -> the first matching line."""
    found = {}
    texts = []
    for f in code_files_for(root, simple):
        with open(os.path.join(root, f), "rb") as fh:
            texts.append(fh.read().decode("latin-1"))
    code = "\n".join(texts)
    if not code:
        return found
    for n in names:
        e = re.escape(n)
        pats = [r"\b%s\s*\.\s*(Location|Left|Top|Bounds|Size|Width|Height|ClientSize)\s*(\+|-)?=(?!=)" % e,
                r"\b%s\s*\.\s*SetBounds\s*\(" % e,
                r"LayoutManager\s*\.\s*Move\w*\(\s*%s\b" % e,
                r"\b%s\s*\.\s*(Location|Size)\s*=\s*new" % e]
        for p in pats:
            m = re.search(p, code)
            if m:
                line = code[code.rfind("\n", 0, m.start()) + 1:code.find("\n", m.start())].strip()
                found[n] = line[:120]
                break
    return found


# ---------------------------------------------------------------------------------------------------------------------------------------
# Measures of one item

def label_width(n):
    return snap_up(n.text_w + 4) if n.text_w else n.w


def check_width(n, text_room=None):
    """The width a check box or radio button needs for its box and its text on one line."""
    lead = GRID["check_box"] + GRID["check_text"] if n.type == "OpenPmsDental.UI.CheckBox" else 18
    return lead + n.text_w + 2 if n.text_w else n.w


def wrapped_lines(text_w, room):
    if room <= 0:
        return 99
    return max(1, int(math.ceil(text_w * 1.06 / room))) if text_w > room else 1


def is_single_line(n):
    return n.kind == "field"


def is_caption_like(n):
    return n.kind == "label"


# ---------------------------------------------------------------------------------------------------------------------------------------
# The cut tree (rows and columns)

class Block:
    """A node of the cut tree: kind 'leaf' (one item), 'v' (rows top to bottom), 'h' (blocks side by side), 'blob' (overlapping items kept as
    they are, moved together)."""

    def __init__(self, kind, items, children=None):
        self.relaxed = False
        self.kind = kind
        self.items = items
        self.children = children or []

    @property
    def x(self):
        return min(i.nx if getattr(i, "spanned", False) else i.x for i in self.items)

    @property
    def y(self):
        return min(i.y for i in self.items)

    @property
    def r(self):
        return max(i.x + i.w for i in self.items)

    @property
    def b(self):
        return max(i.y + i.h for i in self.items)


def ink_x(i):
    """The part of an item's width its text uses: a label or check box drawn right-aligned may run under its neighbour with empty pixels."""
    k = getattr(i, "kind", "")
    if k == "label" and i.text_w and i.text_w + 4 < i.w:
        ta = i.text_align or ""
        tw = i.text_w + 4
        if ta.endswith("Right"):
            return i.x + i.w - tw, tw
        if ta.endswith("Center"):
            return i.x + (i.w - tw) // 2, tw
        return i.x, tw
    if k in ("check", "radio") and i.text_w:
        cw = min(i.w, check_width(i))
        if i.check_align == "MiddleRight":
            return i.x + i.w - cw, cw
        return i.x, cw
    return i.x, i.w


def bound_pair(a, b):
    """True when a is a label that names b, the item right after it on the same height: a cut must not part them."""
    if getattr(a, "kind", "") != "label" or getattr(b, "kind", "") == "label":
        return False
    if min(a.y + a.h, b.y + b.h) - max(a.y, b.y) <= 2:
        return False
    ax, aw = ink_x(a)
    return 0 <= ink_x(b)[0] - (ax + aw) + 6 <= 46


def parts_pair(left, right):
    return any(bound_pair(a, b) for a in left for b in right)


def split(items, axis, tol=2, merge_pairs=True):
    """Splits items along axis ('y' gives rows, 'x' gives columns) where no item spans the gap (items may overlap by tol px)."""
    if axis == "y":
        key = sorted(items, key=lambda i: (i.y, i.x))
        lo = lambda i: i.y
        hi = lambda i: i.y + i.h
    else:
        key = sorted(items, key=lambda i: (ink_x(i)[0], i.y))
        lo = lambda i: ink_x(i)[0]
        hi = lambda i: sum(ink_x(i))
    groups = []
    cur = []
    cur_hi = None
    for i in key:
        if cur and lo(i) >= cur_hi - tol:
            groups.append(cur)
            cur = []
            cur_hi = None
        cur.append(i)
        cur_hi = hi(i) if cur_hi is None else max(cur_hi, hi(i))
    if cur:
        groups.append(cur)
    if axis == "x" and len(groups) > 1 and merge_pairs:
        merged = [groups[0]]
        for gr in groups[1:]:
            if parts_pair(merged[-1], gr):
                merged[-1] = merged[-1] + gr
            else:
                merged.append(gr)
        groups = merged
    return groups


def cut(items):
    if len(items) == 1:
        return Block("leaf", items)
    rows = split(items, "y", tol=5)
    if len(rows) > 1:
        return Block("v", items, [cut(g) for g in rows])
    cols = split(items, "x")
    if len(cols) > 1:
        return Block("h", items, [cut(g) for g in cols])
    # A row whose items overlap a little vertically but not side by side: try the columns with a looser tolerance.
    cols = split(items, "x", tol=6)
    if len(cols) > 1:
        return Block("h", items, [cut(g) for g in cols])
    # Columns that a straight line cannot part (a time bar with a box under it that reaches under the next column's rows): a left part and
    # a right part such that every left item is left of every right item it shares a height with. The parts keep their x.
    # One row whose label and field only the pair rule holds together: side by side.
    raw = split(items, "x", tol=6, merge_pairs=False)
    if len(raw) > 1:
        return Block("h", items, [cut(g) for g in raw])
    parts = relaxed_columns(items)
    if parts:
        b = Block("h", items, [cut(g) for g in parts])
        b.relaxed = True
        return b
    return Block("blob", items)


def relaxed_columns(items, tol=2):
    order = sorted(items, key=lambda i: (ink_x(i)[0] + ink_x(i)[1] / 2.0, i.y))
    for k in range(1, len(order)):
        left, right = order[:k], order[k:]
        ok = True
        for a in left:
            ax, aw = ink_x(a)
            for b in right:
                if min(a.y + a.h, b.y + b.h) - max(a.y, b.y) <= 2:
                    continue
                bx, bw = ink_x(b)
                if ax + aw > bx + tol:
                    ok = False
                    break
            if not ok:
                break
        if ok and not parts_pair(left, right):
            return [left, right]
    return None


def is_row(block):
    """True when the block is one row of single items side by side (or one item)."""
    if block.kind == "leaf":
        return True
    if block.kind == "h":
        return all(c.kind == "leaf" for c in block.children)
    return False


def row_items(block):
    if block.kind == "leaf":
        return list(block.items)
    return [c.items[0] for c in sorted(block.children, key=lambda c: c.x)]


# ---------------------------------------------------------------------------------------------------------------------------------------
# Laying out one container

class Unit:
    """One cell of a row: a label and its field (pair), or one item."""

    def __init__(self, items, row):
        self.items = items
        self.row = row
        self.label = items[0] if len(items) == 2 else None
        self.main = items[-1]
        self.col = None

    @property
    def anchor(self):
        m = self.main
        if getattr(m, "spanned", False):
            return m.nx
        if m.kind in ("check", "radio") and m.check_align == "MiddleRight" and m.text_w:
            return m.x + m.w - GRID["check_box"] - 1
        return m.x


def make_units(items, row_index):
    units = []
    i = 0
    while i < len(items):
        it = items[i]
        nxt = items[i + 1] if i + 1 < len(items) else None
        if (it.kind == "label" and nxt is not None and nxt.kind in ("field", "other", "button", "panel") and nxt.x - (it.x + it.w) <= 40
                and nxt.h <= 3 * GRID["field_height"] and not it.fixed and not nxt.fixed):
            units.append(Unit([it, nxt], row_index))
            i += 2
            continue
        if (it.kind == "label" and nxt is not None and nxt.kind == "check" and not nxt.text_w and nxt.x - (it.x + it.w) <= 40):
            units.append(Unit([it, nxt], row_index))
            i += 2
            continue
        units.append(Unit([it], row_index))
        i += 1
    return units


class Layout:
    def __init__(self, planner, container, content):
        self.p = planner
        self.c = container
        self.left, self.top, self.right, self.bottom = content
        self.problems = []

    # -- sizes by kind ------------------------------------------------------------------------------------------------------------------
    def item_height(self, n, in_field_row):
        k = n.kind
        H = self.p.field_height
        if n.fixed:
            return n.nh
        if k == "field":
            if "ODDatePicker" in n.type or "ComboBoxClinicPicker" in n.type:
                return n.nh
            return H
        if k == "button" and in_field_row and n.h <= H + 6:
            return H
        if k in ("check", "radio"):
            return max(n.nh if n.h > self.p.check_height + 6 else 0, self.p.check_height)
        if k == "label" and in_field_row and n.h <= H + 4:
            return H
        return n.nh

    def set_size(self, n, w, h, rule):
        if n.fixed or not n.editable:
            return
        if (w, h) != (n.nw, n.nh):
            n.nw, n.nh = w, h
            n.add_rule("Size", rule)

    def set_loc(self, n, x, y, rule):
        if n.fixed or not n.editable:
            return
        if (x, y) != (n.nx, n.ny):
            n.nx, n.ny = x, y
            n.add_rule("Location", rule)

    # -- the block tree -------------------------------------------------------------------------------------------------------------------
    def place(self, block, x0, y0, right):
        """Places block with its top at y0 and nothing left of x0; returns (right edge, bottom edge) used."""
        if block.kind == "v":
            return self.place_v(block, x0, y0, right)
        if block.kind == "h":
            if is_row(block):
                return self.place_v(Block("v", block.items, [block]), x0, y0, right)
            return self.place_h(block, x0, y0, right)
        if block.kind == "leaf":
            return self.place_v(Block("v", block.items, [block]), x0, y0, right)
        return self.place_blob(block, x0, y0)

    def move_by(self, n, dx, dy, rule):
        if n.fixed or not n.editable:
            return
        if n.members:
            for m in n.members:
                self.set_loc(m, m.nx + dx, m.ny + dy, rule)
            n.nx += dx
            n.ny += dy
            return
        self.set_loc(n, n.nx + dx, n.ny + dy, rule)

    def place_blob(self, block, x0, y0):
        dx = max(0, x0 - block.x)
        dy = y0 - block.y
        if any(i.fixed for i in block.items):
            dx = dy = 0
        for i in block.items:
            self.move_by(i, dx, dy, "blob: kept as drawn, moved with its row")
        self.problems.append("overlapping items kept as drawn: " + ", ".join(sorted(i.name for i in block.items)))
        return max(i.nx + i.nw for i in block.items), max(i.ny + i.nh for i in block.items)

    def place_h(self, block, x0, y0, right):
        kids = sorted(block.children, key=lambda c: c.x)
        cursor = x0
        bottoms = []
        rights = []
        base_y = block.y
        containers = [k for k in kids if k.kind == "leaf" and k.items[0].kind in ("group", "panel", "tabs")]
        prev_block = None
        for k in kids:
            off_y = k.y - base_y
            ky = y0 + (snap(off_y) if off_y >= 12 else 0)
            gap = GRID["sibling_gap"] if (k.kind == "leaf" and k.items[0].kind in ("group", "panel", "tabs")) else GRID["band_gap"]
            # Blocks keep their x unless the block before now reaches into them; a gap that was tighter than the grid's stays (at least 4).
            if prev_block is not None:
                orig_gap = k.x - prev_block.r
                gap = min(gap, max(4, orig_gap))
                if orig_gap <= GRID["band_gap"] + 8 and orig_gap >= 0 and gap == GRID["sibling_gap"]:
                    gap = GRID["sibling_gap"]
            kx = max(k.x, cursor - GRID["sibling_gap"] - GRID["band_gap"] + gap) if prev_block is not None else max(k.x, x0)
            if block.relaxed:
                kx = max(k.x, x0)
            r, b = self.place(k, kx, ky, right)
            prev_block = k
            cursor = r + GRID["sibling_gap"] + GRID["band_gap"]
            rights.append(r)
            bottoms.append(b)
        # Containers side by side in a row get one height.
        if len(containers) >= 2 and len(containers) == len(kids):
            nodes = [k.items[0] for k in containers]
            tops = {n.ny for n in nodes}
            if len(tops) == 1:
                hmax = max(n.nh for n in nodes)
                for n in nodes:
                    if not (n.anchor & 2 and n.anchor & 1):
                        self.set_size(n, n.nw, hmax, "containers side by side: one height")
                bottoms = [n.ny + n.nh for n in nodes]
        return max(rights), max(bottoms)

    def place_v(self, block, x0, y0, right):
        kids = sorted(block.children, key=lambda c: c.y)
        rows = [k for k in kids if is_row(k)]
        # Units and columns of the rows.
        units_by_row = {}
        all_units = []
        for ri, k in enumerate(kids):
            if is_row(k):
                us = make_units(row_items(k), ri)
                units_by_row[ri] = us
                all_units.extend(us)
        self.assign_columns(all_units, x0, right)
        # Rows top to bottom.
        y = y0
        prev = None
        prev_kind = None
        bottom = y0
        rmax = x0
        for ri, k in enumerate(kids):
            if prev is not None:
                y += self.gap(prev, k, prev_kind)
            if any(i.fixed for i in k.items):
                y = max(y, k.y) if k.y >= y else y
            if is_row(k):
                r, b, kind = self.place_row(units_by_row[ri], y, right)
            else:
                cx = x0
                col_units = [u for u in all_units]
                r, b = self.place(k, max(cx, x0 if k.x - x0 < GRID["column_snap"] else k.x), y, right)
                kind = "block"
            rmax = max(rmax, r)
            bottom = max(bottom, b)
            y = b
            prev = k
            prev_kind = kind
        return rmax, bottom

    def gap(self, prev, cur, prev_kind):
        orig = cur.y - prev.b
        pk = prev_kind
        ck = self.row_kind(cur)
        if orig >= GRID["section_from"]:
            return GRID["section_gap"]
        if pk == "caption" and orig <= 6:
            return GRID["caption_gap"]
        if pk in ("container", "block") or ck in ("container", "block"):
            return GRID["sibling_gap"] if (pk == "container" or ck == "container") else GRID["row_gap"]
        if pk == "list" and ck == "list":
            return GRID["list_gap"]
        return GRID["row_gap"]

    def row_kind(self, block):
        if not is_row(block):
            return "block"
        its = row_items(block)
        kinds = {i.kind for i in its}
        if kinds <= {"check", "radio"}:
            return "list"
        if kinds <= {"group", "panel", "tabs"}:
            return "container"
        if len(its) == 1 and its[0].kind == "label":
            return "caption"
        return "fields"

    def assign_columns(self, units, x0, right):
        """Clusters the units' anchors into columns and gives each column its x."""
        if not units:
            return
        stack_has_pairs = any(u.label is not None for u in units)
        checks = [u for u in units if u.label is None and u.main.kind in ("check", "radio")]
        others = [u for u in units if u not in checks]
        # Columns of the other units by their anchor.
        cols = []
        for u in sorted(others, key=lambda u: u.anchor):
            snap_here = GRID["field_column_snap"] if (u.label is not None and cols and any(v.label is not None for v in cols[-1])) else GRID["column_snap"]
            if cols and u.anchor - cols[-1][-1].anchor <= snap_here:
                cols[-1].append(u)
            else:
                cols.append([u])
        field_cols = [c for c in cols if any(u.label is not None for u in c)]
        # Check boxes: at the first field column when every one fits there, else at the content's left edge (a check box list).
        if checks:
            target = None
            if stack_has_pairs and field_cols:
                fc = field_cols[0]
                fx = self.column_x(fc, x0)
                if all(fx + check_width(u.main) <= right for u in checks):
                    target = fc
            for u in checks:
                if target is not None and u.main.check_align == "MiddleRight":
                    target.append(u)
                elif target is not None and abs(u.anchor - target[0].anchor) <= GRID["column_snap"]:
                    target.append(u)
                else:
                    placed = False
                    for c in cols:
                        if abs(u.anchor - c[0].anchor) <= GRID["column_snap"] and u.main.check_align != "MiddleRight":
                            c.append(u)
                            placed = True
                            break
                    if not placed:
                        u.col = {"x": x0 if (u.main.check_align == "MiddleRight" or u.anchor - x0 < 40) else u.anchor, "units": [u]}
        cols.sort(key=lambda c: min(u.anchor for u in c))
        for c in cols:
            col = {"x": self.column_x(c, x0), "units": c}
            for u in c:
                u.col = col

    def column_x(self, c, x0):
        need = x0
        for u in c:
            if u.label is not None:
                need = max(need, x0 + label_width(u.label) + GRID["label_gap"])
        anchors = [u.anchor for u in c if not (u.main.kind in ("check", "radio") and u.main.check_align == "MiddleRight")]
        orig = min(anchors) if anchors else need
        if orig - x0 < 4:
            orig = x0
        return max(need, orig)

    def place_row(self, units, y, right):
        H = self.p.field_height
        field_row = any(u.main.kind == "field" or u.label is not None for u in units)
        h_row = 0
        for u in units:
            for it in u.items:
                h_row = max(h_row, self.item_height(it, field_row))
        if not units:
            return right, y, "fields"
        # Horizontal: left to right, each unit at its column's x, never before the previous unit's right + gap.
        cursor = None
        rmax = 0
        prev_orig_right = None
        for idx, u in enumerate(sorted(units, key=lambda u: u.anchor)):
            x = u.col["x"] if u.col else u.anchor
            orig_left = min(ink_x(i)[0] for i in u.items)
            if cursor is not None and u.label is None and prev_orig_right is not None and orig_left - prev_orig_right <= 40:
                # An item that belongs to the one before it (a field's button or unit label, a check box's note) follows it.
                x = cursor + GRID["item_gap"]
            elif cursor is not None:
                gap = GRID["item_gap"] if u.label is None else GRID["band_gap"]
                need = cursor + gap + (label_width(u.label) + GRID["label_gap"] if u.label is not None else 0)
                if need > x:
                    x = need
                    if u.col:
                        u.col["x"] = max(u.col["x"], x)
            m = u.main
            # Check boxes: the box on the left.
            if m.kind in ("check", "radio") and m.text_w and not m.fixed:
                if m.check_align == "MiddleRight":
                    m.new_check_align = "MiddleLeft"
                    m.add_rule("CheckAlign", "check box: the box on the left of its text")
                width = check_width(m)
                room = right - x
                hh = self.item_height(m, field_row)
                if width > room and room >= 0.55 * width and room > 60:
                    lines = min(3, wrapped_lines(m.text_w, room - 20))
                    hh = max(hh, lines * (m.text_h or 16) + 4)
                    width = room
                    h_row = max(h_row, hh)
                elif width > room:
                    # No room to wrap into: the text stays on one line and the container grows to it.
                    self.problems.append("check box %s needs %d px, the row has %d: the container grows" % (m.name, width, max(0, room)))
                    room = width + 1
                self.set_size(m, width, hh, "check box: its text on one line" if width < room else "check box: text wraps in a taller row")
                if m.autosize and m.type != "OpenPmsDental.UI.CheckBox":
                    m.autosize_off = True
            elif m.kind == "field" and not m.fixed:
                w = snap(m.nw) if m.nw > 8 else m.nw
                self.set_size(m, w, self.item_height(m, True), "single-line field: the standard height, width on the 4 px grid")
            elif m.kind == "button" and field_row and not m.fixed:
                self.set_size(m, m.nw, self.item_height(m, True), "button in a field row: the field height")
            elif m.kind == "label" and not m.fixed:
                if m.text_w and m.text_w + 4 > m.nw and m.nw < right - x:
                    self.set_size(m, min(label_width(m), right - x), max(m.nh, self.item_height(m, field_row)), "label: its text fits")
                    if m.autosize:
                        m.autosize_off = True
            if u.label is not None:
                lb = u.label
                lw = label_width(lb)
                lx = x - GRID["label_gap"] - lw
                lh = self.item_height(lb, True)
                self.set_size(lb, lw, lh, "label: 6 px before its field, right-aligned")
                if lb.autosize:
                    lb.autosize_off = True
                if lb.text_align not in ("MiddleRight",):
                    lb.new_text_align = "MiddleRight"
                    lb.add_rule("TextAlign", "label: right-aligned, centred on its field")
                self.set_loc(lb, lx, y + (h_row - lh) // 2 if h_row <= H + 2 else y, "label: 6 px before its field, centred on it")
            mh = m.nh
            my = y + (h_row - mh) // 2 if h_row <= 2 * H else y
            if m.members:
                self.move_by(m, x - m.nx, my - m.ny, "stacked controls: moved together")
            else:
                self.set_loc(m, x, my, "row: on the grid")
            cursor = m.nx + m.nw
            if m.kind in ("check", "radio") and m.text_w:
                cursor = m.nx + min(m.nw, check_width(m))
            prev_orig_right = max(sum(ink_x(i)) for i in u.items)
            rmax = max(rmax, m.nx + m.nw)
        kinds = {u.main.kind for u in units}
        kind = "fields"
        if all(u.label is None for u in units):
            if kinds <= {"check", "radio"}:
                kind = "list"
            elif kinds <= {"group", "panel", "tabs"}:
                kind = "container"
            elif len(units) == 1 and units[0].main.kind == "label":
                kind = "caption"
        bottom = y + h_row
        for u in units:
            for it in u.items:
                bottom = max(bottom, it.ny + it.nh)
        return rmax, bottom, kind


# ---------------------------------------------------------------------------------------------------------------------------------------
# The planner

class Planner:
    def __init__(self, root_dir):
        self.root = root_dir
        self.field_height = GRID["field_height"]
        self.check_height = GRID["check_height"]
        self.rows = []
        self.unplaced = []

    def measure(self, tree):
        hs = [n.pref_h for n in walk(tree) if n.type == "System.Windows.Forms.TextBox" and n.pref_h > 0 and not n.multiline]
        if hs:
            self.field_height = collections.Counter(hs).most_common(1)[0][0]
        ths = [n.text_h for n in walk(tree) if n.kind == "check" and n.text_h]
        if ths:
            self.check_height = max(GRID["check_height"], collections.Counter(ths).most_common(1)[0][0] + 2)

    def plan_window(self, window, tree, designer, simple):
        if tree is None:
            self.unplaced.append((window, "", "no control tree"))
            return
        names = [n.name for n in walk(tree) if n.name]
        fixed = runtime_positioned(self.root, simple, [x for x in names if x in designer.declared])
        for n in walk(tree):
            n.editable = n is tree or n.name in designer.declared
            if n is not tree and designer.docked.get(n.name, "None") != "None":
                n.dock = designer.docked[n.name]  # LayoutManager lays docked controls out itself; the designer says what they are
            if n.name in fixed:
                n.fixed = True
                n.why_fixed = "positioned in code: " + fixed[n.name]
            if any(h in n.type for h in OWNER_DRAWN_HINTS):
                n.fixed = True
                n.why_fixed = "owner-drawn surface"
            if n.editable and n is not tree and n.parent is not None and n.parent is not tree and designer.adds.get(n.name) not in (None, n.parent.name):
                n.editable = False
        # The tree must be the build of these designer files: a plan made from a stale tree would move controls by the wrong amounts.
        compared = mismatched = 0
        for n in walk(tree):
            if n is tree or not n.editable or n.dock not in ("None", ""):
                continue
            loc = designer.point(n.name)
            if loc:
                compared += 1
                if loc != (n.x, n.y):
                    mismatched += 1
        if compared and mismatched > 0.3 * compared:
            self.unplaced.append((window, ".", "the control tree is not the build of the designer file (%d of %d locations differ): dump again" % (mismatched, compared)))
            for n in walk(tree):
                n.editable = False
            return
        self.layout_tree(tree, is_root=True, window=window)
        # Rows of the plan.
        for n in walk(tree):
            if not n.editable or n.fixed:
                continue
            if n is not tree and n.dock not in ("None", ""):
                continue
            if n is tree:
                if (n.nw, n.nh) != (n.w, n.h):
                    prop = "ClientSize" if designer.size(".", "ClientSize") else "Size"
                    self.add(designer, window, ".", prop, "%d,%d" % (n.w, n.h), "%d,%d" % (n.nw, n.nh), n.rule.get("Size", "window grows"))
                continue
            loc = designer.point(n.name)
            if "Location" in n.rule and (n.nx, n.ny) != (n.x, n.y):
                old = "%d,%d" % loc if loc else "(none)"
                if loc and loc != (n.x, n.y):
                    nx, ny = loc[0] + n.nx - n.x, loc[1] + n.ny - n.y
                else:
                    nx, ny = n.nx, n.ny
                self.add(designer, window, n.name, "Location", old, "%d,%d" % (nx, ny), n.rule["Location"])
            if "Size" in n.rule and (n.nw, n.nh) != (n.w, n.h):
                sz = designer.size(n.name)
                old = "%d,%d" % sz if sz else "(none)"
                self.add(designer, window, n.name, "Size", old, "%d,%d" % (n.nw, n.nh), n.rule["Size"])
            if n.autosize_off and ("Size" in n.rule):
                self.add(designer, window, n.name, "AutoSize", "true", "false", "a size set by the grid")
            if n.new_check_align:
                self.add(designer, window, n.name, "CheckAlign", n.check_align, n.new_check_align, n.rule["CheckAlign"])
            if n.new_text_align and n.type in LABEL_TYPES:
                self.add(designer, window, n.name, "TextAlign", n.text_align, n.new_text_align, n.rule["TextAlign"])

    def check(self, window, tree):
        """The planned layout's own problems: siblings that overlap, controls past their container's client edge."""
        out = []
        for c in walk(tree):
            kids = [k for k in c.children if k.dock in ("None", "") and k.visible and not k.in_layout_panel and c.kind not in ("tabs",)]
            for i, a in enumerate(kids):
                for b in kids[i + 1:]:
                    ax, aw = ink_x_new(a)
                    bx, bw = ink_x_new(b)
                    ix = min(ax + aw, bx + bw) - max(ax, bx)
                    iy = min(a.ny + a.nh, b.ny + b.nh) - max(a.ny, b.ny)
                    if ix > 0 and iy > 0 and overlap_ratio(a, b) < 0.6:
                        out.append((window, c.name or ".", "overlap %s %s by %dx%d" % (a.name, b.name, ix, iy)))
                if c is not tree or True:
                    if a.nx + a.nw > c.nw or a.ny + a.nh > c.nh or a.nx < 0 or a.ny < 0:
                        if c.kind not in ("other",):
                            out.append((window, c.name or ".", "past the edge: %s %d,%d %dx%d in %dx%d" % (a.name, a.nx, a.ny, a.nw, a.nh, c.nw, c.nh)))
        return out

    def add(self, designer, window, control, prop, old, new, rule):
        self.rows.append((designer.rel, window, control, prop, old, new, rule))

    def content_rect(self, n, is_root):
        pad = GRID["pad"]
        top = pad
        if n.kind == "group":
            top = (n.title_bottom + GRID["title_gap"]) if n.title_bottom else pad
        right = n.nw - pad
        bottom = n.nh - pad
        left = pad
        # Docked children take their edge.
        for c in n.children:
            if c.dock == "Top":
                top = max(top, c.y + c.h + pad)
            elif c.dock == "Bottom":
                bottom = min(bottom, c.y - pad)
            elif c.dock == "Left":
                left = max(left, c.x + c.w + pad)
            elif c.dock == "Right":
                right = min(right, c.x - pad)
        return left, top, right, bottom

    def layout_tree(self, n, is_root, window):
        # Containers that nearly span their parent's content span it exactly (equal margins left and right), before their own content is laid out.
        if self.lays_out(n) and n.kind not in ("tabs",):
            left, top, right, bottom = self.content_rect(n, is_root)
            for c in n.children:
                if (c.kind in ("group", "panel") and c.editable and not c.fixed and c.dock in ("None", "") and abs(c.x - left) <= GRID["span_snap"]
                        and abs(c.x + c.w - right) <= GRID["span_snap"] and not (c.anchor & 8 and not c.anchor & 4)):
                    if (c.nx, c.nw) != (left, right - left):
                        c.nw = right - left
                        c.add_rule("Size", "container: spans its parent's content, 8 px from each side")
                    c.nx = left
                    c.spanned = True
                    if c.x != left:
                        c.add_rule("Location", "container: spans its parent's content, 8 px from each side")
        # Children first: a container's size comes from its content.
        for c in n.children:
            if c.children and c.editable and not c.fixed and c.kind in ("group", "panel", "tabs", "tabpage", "other") and self.lays_out(c):
                self.layout_tree(c, False, window)
        if not self.lays_out(n):
            return
        if n.kind == "tabs":
            # The pages share the tab control's size: it grows to the largest page's need.
            need_w = max([p.nw for p in n.children] + [0])
            need_h = max([p.nh for p in n.children] + [0])
            return
        movable = [c for c in n.children if c.dock in ("None", "") and not c.in_layout_panel]
        if not movable:
            return
        items = merge_stacked(movable)
        left, top, right, bottom = self.content_rect(n, is_root)
        lay = Layout(self, n, (left, top, right, bottom))
        bottom_items = []
        if is_root:
            bottom_items = [i for i in items if i.anchored_bottom_only and i.y + i.h > n.h - 60]
            items = [i for i in items if i not in bottom_items]
        if items:
            block = cut(items)
            r, b = lay.place(block, left, top, right)
        else:
            r, b = left, top
        need_h = b + GRID["pad"]
        need_w = r + GRID["pad"]
        if bottom_items:
            bb = Block("v", bottom_items)
            h_bottom = max(i.y + i.h for i in bottom_items) - min(i.y for i in bottom_items)
            need_h = b + GRID["section_gap"] + h_bottom + GRID["pad"]
        grow_w = max(n.nw, need_w)
        if n.kind == "group" and not is_root:
            new_h = need_h if not (n.anchor & 1 and n.anchor & 2) else max(n.nh, need_h)
        else:
            new_h = max(n.nh, need_h)
        if is_root and window.split(".")[-1].startswith("UserControl"):
            grow_w = n.nw  # Preferences pages keep their width
        if (grow_w, new_h) != (n.nw, n.nh):
            if n is not None and (n.editable or is_root):
                n.nw, n.nh = grow_w, new_h
                n.add_rule("Size", "container: its content and padding" if not is_root else "window grows to its content")
        if bottom_items:
            self.place_bottom(lay, n, bottom_items)
        self.align_checks(lay, n)
        for pr in lay.problems:
            self.unplaced.append((window, n.name or ".", pr))
        # A tab page that grew makes its tab control grow by the same.
        if n.kind == "tabpage" and n.parent is not None and n.parent.kind == "tabs":
            t = n.parent
            dw, dh = n.nw - n.cw if False else 0, 0
            if n.nh > n.h or n.nw > n.w:
                dh = n.nh - n.h
                dw = n.nw - n.w
                if t.editable and not t.fixed:
                    nw, nh = max(t.nw, t.w + dw), max(t.nh, t.h + dh)
                    if (nw, nh) != (t.nw, t.nh):
                        t.nw, t.nh = nw, nh
                        t.add_rule("Size", "tab control: its largest page's content")
                n.rule.pop("Size", None)  # a page's size follows its tab control: no Size line is written for it

    def align_checks(self, lay, n):
        """A check box that was drawn with its box on the field column, but whose rows the cut parted from the label + field rows, still
        puts its box on that column (with the note that follows it) when nothing is in the way."""
        kids = [c for c in n.children if c.dock in ("None", "")]
        xs = collections.Counter()
        for c in kids:
            if c.kind == "label" and "TextAlign" in c.rule or (c.kind == "label" and (c.text_align or "").endswith("Right")):
                for f in kids:
                    if f.kind == "field" and abs(f.ny - c.ny) <= 6 and 0 <= f.nx - (c.nx + c.nw) <= 8:
                        xs[f.nx] += 1
        if not xs:
            return
        fx = xs.most_common(1)[0][0]
        for c in kids:
            if c.kind not in ("check", "radio") or c.new_check_align is None or c.fixed or not c.editable:
                continue
            box = c.x + c.w - GRID["check_box"] - 1
            if abs(box - fx) > 24 or c.nx >= fx - 2:
                continue
            dx = fx - c.nx
            followers = [f for f in kids if f is not c and min(f.y + f.h, c.y + c.h) - max(f.y, c.y) > 2 and 0 <= f.x - (c.x + c.w) <= 40]
            moved = [c] + followers
            ok = True
            for m in moved:
                for o in kids:
                    if o in moved or not o.visible:
                        continue
                    if (min(m.nx + dx + min(m.nw, check_width(m) if m.kind in ("check", "radio") else m.nw), o.nx + o.nw) - max(m.nx + dx, o.nx) > 0
                            and min(m.ny + m.nh, o.ny + o.nh) - max(m.ny, o.ny) > 0):
                        ok = False
            if not ok or max(m.nx + dx + m.nw for m in moved) > n.nw - GRID["pad"]:
                continue
            for m in moved:
                lay.set_loc(m, m.nx + dx, m.ny, "check box: its box on the field column")

    def lays_out(self, n):
        if n.fixed:
            return False
        if any(h in n.type for h in OWNER_DRAWN_HINTS):
            return False
        if n.type in ("System.Windows.Forms.FlowLayoutPanel", "System.Windows.Forms.TableLayoutPanel", "System.Windows.Forms.SplitContainer",
                      "System.Windows.Forms.SplitterPanel"):
            return False
        if any(c.dock == "Fill" for c in n.children):
            return False
        # Only containers whose children this designer adds.
        return any(c.editable for c in n.children)

    def place_bottom(self, lay, n, items):
        """The window's bottom bar: anchored to the bottom, 8 px from the bottom edge; dialog buttons bottom right, 8 px apart."""
        pad = GRID["pad"]
        b_orig = max(i.y + i.h for i in items)
        dy = (n.nh - pad) - b_orig
        dialog = sorted([i for i in items if i.name in DIALOG_NAMES and i.anchor & 8], key=lambda i: i.x)
        others = [i for i in items if i not in dialog]
        for i in others:
            lay.move_by(i, 0, dy, "bottom bar: 8 px from the window's bottom")
        x = n.nw - pad
        for i in reversed(dialog):
            x -= i.nw
            lay.set_loc(i, x, n.nh - pad - i.nh, "dialog button: bottom right, 8 px apart and from the edges")
            x -= GRID["dialog_gap"]


def merge_stacked(items):
    """Items stacked on (nearly) the same bounds are switched at run time: one group item moved together."""
    out = []
    used = set()
    for i, a in enumerate(items):
        if id(a) in used:
            continue
        group = [a]
        for b in items[i + 1:]:
            if id(b) in used:
                continue
            if overlap_ratio(a, b) >= 0.6:
                group.append(b)
                used.add(id(b))
        if len(group) == 1:
            out.append(a)
            continue
        g = GroupItem(group)
        out.append(g)
    return out


def overlap_ratio(a, b):
    ix = max(0, min(a.x + a.w, b.x + b.w) - max(a.x, b.x))
    iy = max(0, min(a.y + a.h, b.y + b.h) - max(a.y, b.y))
    small = min(a.w * a.h, b.w * b.h)
    if small <= 0:
        return 0
    return ix * iy / float(small)


class GroupItem:
    """Controls stacked on the same bounds (switched by Visible at run time): one item for the layout."""

    def __init__(self, members):
        self.members = members
        self.x = min(m.x for m in members)
        self.y = min(m.y for m in members)
        self.w = max(m.x + m.w for m in members) - self.x
        self.h = max(m.y + m.h for m in members) - self.y
        self.nx, self.ny, self.nw, self.nh = self.x, self.y, self.w, self.h
        self.name = "+".join(m.name for m in members)
        self.type = "stacked"
        self.fixed = any(m.fixed for m in members)
        self.editable = all(m.editable for m in members)
        self.anchor = members[0].anchor
        self.check_align = ""
        self.text_w = 0
        self.text_h = 0
        self.autosize = False
        self.rule = collections.OrderedDict()
        self.kind = "stacked"
        self.multiline = False
        self.text_align = ""
        self.new_check_align = None
        self.new_text_align = None
        self.autosize_off = False

    @property
    def anchored_bottom_only(self):
        return bool(self.anchor & 2) and not (self.anchor & 1)

    def add_rule(self, prop, rule):
        pass


def ink_x_new(i):
    """ink_x for the planned bounds."""
    saved = (i.x, i.w)
    i.x, i.w = i.nx, i.nw
    try:
        if i.new_check_align:
            ca = i.check_align
            i.check_align = i.new_check_align
            r = ink_x(i)
            i.check_align = ca
            return r
        return ink_x(i)
    finally:
        i.x, i.w = saved


def walk(n):
    yield n
    for c in n.children:
        for x in walk(c):
            yield x


def cmd_plan(args):
    root = fit_tools.repo_root()
    # The windows and their mode: tool (the planner lays it out) or hand (listed; its edits are in grid-hand.tsv).
    modes = collections.OrderedDict()
    for r in fit_tools.read_tsv(os.path.join(root, args.windows_file)):
        modes[r["window"]] = (r.get("mode") or "tool", r.get("note") or "")
    windows = set(modes.keys())
    if args.windows:
        windows = set(args.windows.split(","))
    trees = load_tree(args.tree, windows)
    wf = fit_tools.window_files(root, list(trees.keys()))
    planner = Planner(root)
    for w, tree in trees.items():
        mode, note = modes.get(w, ("tool", ""))
        if mode != "tool":
            planner.unplaced.append((w, ".", "by hand: " + note))
            continue
        files = wf.get(w) or []
        if not files:
            planner.unplaced.append((w, "", "no designer file"))
            continue
        designer = Designer(root, files[0])
        planner.measure(tree)
        planner.plan_window(w, tree, designer, w.split(".")[-1])
        problems = planner.check(w, tree)
        planner.unplaced.extend(problems)
        if problems:
            print("%-45s %d planned-layout problems" % (w, len(problems)))
    with open(args.out, "w", encoding="utf-8", newline="\n") as f:
        f.write("designer_file\twindow\tcontrol\tproperty\told\tnew\trule\n")
        for r in planner.rows:
            f.write("\t".join(r) + "\n")
    if args.unplaced:
        with open(args.unplaced, "w", encoding="utf-8", newline="\n") as f:
            f.write("window\tcontrol\twhy\n")
            for r in planner.unplaced:
                f.write("\t".join(x.replace("\t", " ") for x in r) + "\n")
    print("rows", len(planner.rows), "windows", len(trees), "listed", len(planner.unplaced))


# ---------------------------------------------------------------------------------------------------------------------------------------
# The applier

def line_end_at(text, pos):
    e = text.find("\n", pos)
    if e < 0:
        return ""
    return "\r\n" if e > 0 and text[e - 1] == "\r" else "\n"


def insert_before_name_line(text, name, stmt):
    pat = re.compile(r"^([ \t]*)this\.%s\.Name ?= ?[^\r\n]*?(\r\n|\n)" % re.escape(name), re.M)
    m = pat.search(text)
    if not m:
        return text, "no Name line"
    return text[:m.start()] + m.group(1) + stmt + m.group(2) + text[m.start():], None


def apply_row(text, control, prop, new):
    target = "this." + control + "." if control != "." else "this."
    if prop in ("Location",):
        x, y = new.split(",")
        pat = re.compile(r"(%sLocation ?= ?new System\.Drawing\.Point\()(-?\d+)(, ?)(-?\d+)(\))" % re.escape(target))
        ms = list(pat.finditer(text))
        if len(ms) == 1:
            m = ms[0]
            return text[:m.start(2)] + x + m.group(3) + y + text[m.end(4):], None
        if not ms and control != ".":
            return insert_before_name_line(text, control, "this.%s.Location = new System.Drawing.Point(%s, %s);" % (control, x, y))
        return text, "Location line found %d times" % len(ms)
    if prop in ("Size", "ClientSize"):
        w, h = new.split(",")
        pat = re.compile(r"(%s%s ?= ?new System\.Drawing\.Size\()(\d+)(, ?)(\d+)(\))" % (re.escape(target), prop))
        ms = list(pat.finditer(text))
        if len(ms) == 1:
            m = ms[0]
            return text[:m.start(2)] + w + m.group(3) + h + text[m.end(4):], None
        if not ms and control != ".":
            return insert_before_name_line(text, control, "this.%s.Size = new System.Drawing.Size(%s, %s);" % (control, w, h))
        return text, "%s line found %d times" % (prop, len(ms))
    if prop == "AutoSize":
        pat = re.compile(r"(%sAutoSize ?= ?)(true|false)(;)" % re.escape(target))
        m = pat.search(text)
        if m:
            return text[:m.start(2)] + new + text[m.end(2):], None
        return insert_before_name_line(text, control, "this.%s.AutoSize = %s;" % (control, new))
    if prop in ("CheckAlign", "TextAlign"):
        pat = re.compile(r"(%s%s ?= ?System\.Drawing\.ContentAlignment\.)(\w+)(;)" % (re.escape(target), prop))
        m = pat.search(text)
        if m:
            return text[:m.start(2)] + new + text[m.end(2):], None
        if prop == "CheckAlign" and new == "MiddleLeft":
            return text, None  # the default
        return insert_before_name_line(text, control, "this.%s.%s = System.Drawing.ContentAlignment.%s;" % (control, prop, new))
    return text, "unknown property " + prop


def cmd_apply(args):
    root = fit_tools.repo_root()
    rows = fit_tools.read_tsv(args.plan)
    by_file = collections.OrderedDict()
    for r in rows:
        by_file.setdefault(r["designer_file"], []).append(r)
    errors = 0
    changed = 0
    for rel, rs in by_file.items():
        p = os.path.join(root, rel)
        with open(p, "rb") as f:
            raw = f.read()
        text = raw.decode("latin-1")
        for r in rs:
            text2, err = apply_row(text, r["control"], r["property"], r["new"])
            if err:
                errors += 1
                print("%s %s %s: %s" % (rel, r["control"], r["property"], err))
                continue
            text = text2
        data = text.encode("latin-1")
        if data != raw:
            with open(p, "wb") as f:
                f.write(data)
            changed += 1
    print("files changed", changed, "errors", errors)


def main():
    p = argparse.ArgumentParser()
    sub = p.add_subparsers(dest="cmd", required=True)
    a = sub.add_parser("plan")
    a.add_argument("--tree", required=True)
    a.add_argument("--windows")
    a.add_argument("--windows-file", default="tests/UiRefreshTests/Data/grid-windows.tsv")
    a.add_argument("--out", required=True)
    a.add_argument("--unplaced")
    b = sub.add_parser("apply")
    b.add_argument("--plan", required=True)
    args = p.parse_args()
    {"plan": cmd_plan, "apply": cmd_apply}[args.cmd](args)


if __name__ == "__main__":
    main()
