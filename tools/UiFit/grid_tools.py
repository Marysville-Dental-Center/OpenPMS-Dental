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
    "row_gap": 4,             # between field rows (the owner, 2026-11-21: fields 23 px, row pitch 27)
    "list_gap": 4,            # between consecutive check boxes or radio buttons
    "caption_gap": 2,         # a caption label above the control it names
    "section_gap": 12,        # rows that were clearly apart (a gap of section_from or more) stay apart by this
    "section_from": 12,       # not above section_gap: a gap the grid made stays what it is on the next run
    "sibling_gap": 8,         # between containers side by side or stacked
    "item_gap": 6,            # between items of one row (a field and its button or its unit label)
    "band_gap": 16,           # between two label + field bands of one row
    "edge_snap": 16,          # right edges of one column within this share one edge
    "edge_grow": 32,          # polish round: a wide field this much short of its column's right edge grows to it (never shrinks this far)
    "column_snap": 16,        # left edges (anchors) within this are one column
    "field_column_snap": 40,  # ... and for the fields of label + field rows (a label column right-aligned to them)
    "span_snap": 32,          # a container within this of its parent's content edges spans the content
    "snap": 4,                # widths snap to this
    "field_height": 23,       # a single-line field; replaced by the measured height of a text box at the program's font
    "check_height": 18,       # a check box or radio button row (UI.CheckBox's own height); at least the text line + 2
    "check_box": 13,          # UI.CheckBox's box (12 + the 1 px line)
    "check_text": 3,          # between the box and the text
    "dialog_gap": 8,          # between dialog buttons
    "scroll_bar": 17,         # a vertical scroll bar at 96 dpi (SystemInformation.VerticalScrollBarWidth)
    "label_lead": 16,         # Preferences style: a label starts where the check box text beside it starts (UI.CheckBox: 13 px box + 3)
    "label_cap": 0.5,         # Preferences style: the field column stays within this share of the content width; a longer label wraps
    "wide_field": 120,        # Preferences style: a text field this wide or wider ends at the content's right edge, like the combo boxes
    "hint_snap": 40,          # field columns of different containers within this (window coordinates) share one left edge
    "check_slack": 3,         # a check box whose one line needs at most this much more than its row has stays one line (GDI+ measures 2-3 px wide)
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
                 "autosize_off", "rule", "members", "spanned", "rtl", "w0", "keep_size")

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
        self.rtl = False
        self.keep_size = False
        self.w0 = self.w  # the width as the tree has it: a field narrowed in one pass widens again when its container grew in a later one

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
        # A multi-line text box drawn one line tall (FormPatientEdit's Referred From, FormScreenEdit's Birthdate) is a single-line field: the
        # standard height and the shared single-line look (UiInputFrame frames a multi-line box of one line too; UI-12 fix round).
        if any(h in short for h in SINGLE_LINE_HINTS) and (not self.multiline or self.h <= 24) and self.h <= 30:
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
        # A control whose Name differs from its field (this.checkLong.Name = "checkShort"): the tree has the Name, the designer the field.
        self.name_to_field = {m.group(2): m.group(1) for m in re.finditer(r'this\.(\w+)\.Name ?= ?"(\w+)";', self.text) if m.group(1) != m.group(2)}
        # Check boxes and radio buttons drawn right to left (RightToLeft.Yes): their box or circle is on the right like CheckAlign MiddleRight.
        self.rtl = set(re.findall(r"this\.(\w+)\.RightToLeft ?= ?System\.Windows\.Forms\.RightToLeft\.Yes;", self.text))
        self.no_label = set(re.findall(r"this\.(\w+)\.ShowLabel ?= ?false;", self.text))
        # Clinic pickers that select several clinics: their label reads "Clinics" (slice 1: the label area is sized from its text).
        self.multi_pickers = set(re.findall(r"this\.(\w+)\.IsMultiSelect ?= ?true;", self.text))
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


def multi_pickers_in_code(root, simple):
    """Clinic pickers the window's code sets to select several clinics (comboClinics.IsMultiSelect=true; in Load): their label reads
    "Clinics" like a designer's IsMultiSelect (FormRpTreatmentFinder)."""
    found = set()
    for f in code_files_for(root, simple):
        with open(os.path.join(root, f), "rb") as fh:
            found.update(re.findall(r"\b(\w+)\s*\.\s*IsMultiSelect\s*=\s*true\s*;", fh.read().decode("latin-1")))
    return found


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
    # Commented-out code positions nothing (//textNote.Width = 400;): the rest of a line after // that is not inside a string goes.
    code = re.sub(r'(?m)^((?:[^"\n/]|"(?:[^"\\\n]|\\.)*"|/(?!/))*)//[^\n]*', r"\1", code)
    for n in names:
        e = re.escape(n)
        pats = [r"\b%s\s*\.\s*(Location|Left|Top|Bounds|Size|Width|Height|ClientSize)\s*(\+|-)?=(?!=)" % e,
                r"\b%s\s*\.\s*SetBounds\s*\(" % e,
                r"LayoutManager\s*\.\s*Move\w*\(\s*%s\b" % e,
                r"\b%s\s*\.\s*(Location|Size)\s*=\s*new" % e]
        all_ms = []
        for p in pats:
            all_ms.extend(re.finditer(p, code))
        all_ms = [m for m in all_ms if not re.match(r"LayoutManager\s*\.\s*Move\w*\(\s*%s\s*,\s*new\s+Point\(\s*%s\.Location\.X[^;]*%s\.Location\.Y\s*\)\s*\)" % (e, e, e),
                                                      code[m.start():])]
        if all_ms and all(re.match(r"(LayoutManager\s*\.\s*Move(Height|Width|Size)\b|\b%s\s*\.\s*(Width|Height|Size)\b)" % e, code[m.start():]) for m in all_ms):
            # Only sized in code (MoveHeight, Width =): the control moves with its row and keeps its size.
            m = all_ms[0]
            found[n] = "size: " + code[code.rfind("\n", 0, m.start()) + 1:code.find("\n", m.start())].strip()[:110]
            continue
        for p in pats:
            ms = list(re.finditer(p, code))
            # A move relative to the control's own place (LayoutManager.MoveLocation(x,new Point(x.Location.X-...,x.Location.Y))) moves it
            # from wherever the designer puts it: the control is laid out with its row.
            ms = [m for m in ms if not re.match(r"LayoutManager\s*\.\s*Move\w*\(\s*%s\s*,\s*new\s+Point\(\s*%s\.Location\.X[^;]*%s\.Location\.Y\s*\)\s*\)" % (e, e, e),
                                             code[m.start():])]
            m = ms[0] if ms else None
            if m:
                line = code[code.rfind("\n", 0, m.start()) + 1:code.find("\n", m.start())].strip()
                found[n] = line[:120]
                break
    return found


def runtime_window_sized(root, simple):
    """The lines of the window's code that size the window itself at run time (Width, Height, Size, ClientSize or Bounds set on the form, or
    LayoutManager.MoveSize/MoveWidth/MoveHeight on this): the sizes they set were chosen for the layout on main, so a rollout slice lists the
    window and checks each one against the new layout (UI-12 fix round: FormInsBenefits' simplified view width, FormPayment's hidden splits
    height)."""
    texts = []
    for f in code_files_for(root, simple):
        with open(os.path.join(root, f), "rb") as fh:
            texts.append(fh.read().decode("latin-1"))
    code = "\n".join(texts)
    code = re.sub(r'(?m)^((?:[^"\n/]|"(?:[^"\\\n]|\\.)*"|/(?!/))*)//[^\n]*', r"\1", code)
    out = []
    pats = [r"(?m)^[ \t]*(?:this\s*\.\s*)?(Width|Height|Size|ClientSize|Bounds)\s*(\+|-)?=(?!=)",
            r"LayoutManager\s*\.\s*Move(Size|Width|Height)\s*\(\s*this\s*,"]
    for p in pats:
        for m in re.finditer(p, code):
            line = code[code.rfind("\n", 0, m.start()) + 1:code.find("\n", m.start())].strip()
            if line not in out:
                out.append(line[:120])
    return out


# ---------------------------------------------------------------------------------------------------------------------------------------
# Measures of one item

def label_width(n):
    # A label drawn on two or more lines keeps the width it wraps at (it stays a wrapped label, not one long line that pushes its field
    # column across the window).
    if n.text_w and n.text_h and n.h >= 1.6 * n.text_h and n.w > 40 and n.text_w + 4 > n.w:
        return n.w
    return snap_up(n.text_w + 4) if n.text_w else n.w


def pair_label_width(lb):
    """The width of what names a field: a label's text, or a button's own width."""
    return lb.nw if lb.kind == "button" else label_width(lb)


def check_width(n, text_room=None):
    """The width a check box or radio button needs for its box and its text on one line."""
    # UI.CheckBox and, from round 3, the stock radio button (UiRadioLook) put the text 16 px from the left; the stock check box 18 px.
    lead = GRID["check_box"] + GRID["check_text"] if n.type in ("OpenPmsDental.UI.CheckBox",) + RADIO_TYPES else 18
    return lead + n.text_w + 2 if n.text_w else n.w


def wrapped_lines(text_w, room):
    if room <= 0:
        return 99
    return max(1, int(math.ceil(text_w * 1.06 / room))) if text_w > room else 1


def note_lines(text, text_w, room):
    """The lines a wrapped note draws in room px: a greedy word wrap of its text with the average character width of its measured one-line
    width (4% wider, for the words that break early), paragraph by paragraph (UI-12 fix round: a note keeps no empty lines under its text)."""
    text = (text or "").replace("\r", "")
    flat = text.replace("\n", " ")
    if not flat.strip() or not text_w or room <= 0:
        return wrapped_lines(text_w, room)
    avg = text_w * 1.04 / max(1, len(flat))
    lines = 0
    for para in text.split("\n"):
        cur = 0.0
        n = 1
        for wd in para.split(" "):
            ww = len(wd) * avg
            if cur > 0 and cur + avg + ww > room:
                n += 1
                cur = ww
            else:
                cur = cur + (avg if cur > 0 else 0) + ww
        lines += n
    if "\n" in text:
        return lines
    return max(lines, 1)


# Segoe UI 9 pt at 96 dpi: the advance of each printable ASCII character (32-126) as a label draws it (GDI+, typographic; measured with
# Graphics.MeasureString, 20 of each). The program's labels draw about 6 px narrower than the dump's one-line width (TextRenderer, with its
# padding): measured_wrap scales the table to that width less 6, so a label in another font or size wraps with its own proportions.
_SEGOE9 = [3.29, 3.41, 4.71, 7.09, 6.47, 9.82, 9.6, 2.76, 3.62, 3.62, 5, 8.21, 2.6, 4.8, 2.6, 4.68, 6.47, 6.47, 6.47, 6.47, 6.47, 6.47,
           6.47, 6.47, 6.47, 6.47, 2.6, 2.6, 8.21, 8.21, 8.21, 5.38, 11.46, 7.74, 6.88, 7.43, 8.41, 6.07, 5.86, 8.23, 8.52, 3.19, 4.28,
           6.96, 5.65, 10.78, 8.98, 9.05, 6.72, 9.05, 7.18, 6.37, 6.29, 8.24, 7.45, 11.21, 7.08, 6.63, 6.84, 3.62, 4.55, 3.62, 8.21, 4.98,
           3.22, 6.11, 7.05, 5.54, 7.07, 6.28, 3.76, 7.07, 6.79, 2.91, 2.91, 5.96, 2.91, 10.34, 6.79, 7.03, 7.05, 7.07, 4.17, 5.09, 4.07,
           6.79, 5.75, 8.67, 5.51, 5.81, 5.43, 3.62, 2.87, 3.62, 8.21]


def _adv(s):
    return sum(_SEGOE9[ord(ch) - 32] if 32 <= ord(ch) <= 126 else 7 for ch in s)


def measured_wrap(text, text_w, room):
    """The lines a label draws when its text wraps in room px (UI-12 fix round 2: measured, word by word): a greedy word wrap with the
    width of each word from the character table above, scaled to the measured one-line width. A word with a hyphen that does not fit
    breaks before the hyphen when its first part fits (as the label draws it: "non" / "-primary"). Paragraph by paragraph; a line end
    after the text draws nothing (UserControlMainWindowMisc's labels end in one)."""
    text = (text or "").replace("\r", "").rstrip()
    flat = text.replace("\n", " ")
    base = _adv(flat)
    if not flat.strip() or not text_w or text_w <= 12 or room <= 0 or base <= 0:
        return None
    k = (text_w - 6) / float(base)

    def w(s):
        return _adv(s) * k

    lines = []
    for para in text.split("\n"):
        cur = ""
        for wd in para.split(" "):
            cand = (cur + " " + wd) if cur else wd
            if not cur or w(cand) <= room:
                cur = cand
                continue
            parts = re.split(r"(?=-)", wd)
            fit = 0
            for i in range(1, len(parts)):
                if parts[i - 1] and w(cur + " " + "".join(parts[:i])) <= room:
                    fit = i
            if fit:
                lines.append(cur + " " + "".join(parts[:fit]))
                cur = "".join(parts[fit:])
            else:
                lines.append(cur)
                cur = wd
        lines.append(cur)
    return lines


def measured_lines(text, text_w, room):
    """The number of lines of measured_wrap in room px less 2 of slack; the word-wrap estimate where the text is not measurable."""
    lines = measured_wrap(text, text_w, room - 2)
    if lines is None:
        return note_lines(text, text_w, room)
    return max(1, len(lines))


def hyphen_safe_width(text, text_w, lw, pad=0):
    """A wrap width at which no line starts with a hyphen (UI-12 fix round 2: "non" / "-primary" read as broken): when a line of the
    wrap at lw would start with '-', the width narrows to halfway between the line without the hyphenated word and the line with its
    first part, so the whole word moves to the next line. Returns lw when nothing starts with a hyphen. The check wraps at 2 px more than
    the width (a line the estimate puts just over the edge may still fit when drawn: PPO write-off description (blank for "Write)."""
    for _ in range(4):
        lines = measured_wrap(text, text_w, lw - pad + 2)
        if not lines or not any(ln.startswith("-") for ln in lines[1:]):
            return lw
        flat = (text or "").replace("\r", "").rstrip().replace("\n", " ")
        k = (text_w - 6) / float(_adv(flat))
        i = next(j for j in range(1, len(lines)) if lines[j].startswith("-"))
        prev = lines[i - 1]
        cut = prev.rfind(" ")
        if cut <= 0:
            return lw
        short = _adv(prev[:cut]) * k
        long_ = _adv(prev) * k
        lw = int(math.floor((short + long_) / 2.0)) + pad
    return lw


def is_single_line(n):
    return n.kind == "field"


def is_separator(block):
    """A row that is only a visible separator line (a panel or label 1-3 px tall, FormInsBenefits' category rules)."""
    if not is_row(block):
        return False
    its = row_items(block)
    return bool(its) and all(i.h <= 3 and i.w >= 20 and getattr(i, "visible", True) for i in its)


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
    if getattr(i, "members", None):
        parts = [ink_x(m) for m in i.members if m.visible] or [ink_x(m) for m in i.members]
        lo = min(p[0] for p in parts)
        return lo, max(p[0] + p[1] for p in parts) - lo
    if "ODDatePicker" in getattr(i, "type", ""):
        # ODDatePicker lays its date box out itself, 63 px from its left edge (OnSizeChanged): the box is what the column aligns.
        return i.x + DATE_BOX_X, min(i.w - DATE_BOX_X, DATE_BOX_W)
    if "ComboBoxClinicPicker" in getattr(i, "type", "") and getattr(i, "name", "") not in NO_LABEL_PICKERS:
        # The clinic picker draws its own "Clinic" label in its label area and its box after it (clinic_label_w).
        lw = clinic_label_w(i)
        return i.x + lw, i.w - lw
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
    if getattr(a, "kind", "") != "label" or getattr(b, "kind", "") in ("label", "group", "tabs", "tabpage"):
        return False
    if min(a.y + a.h, b.y + b.h) - max(a.y, b.y) <= 2:
        return False
    ax, aw = ink_x(a)
    if 0 <= ink_x(b)[0] - (ax + aw) + 6 <= 46:
        return True
    # A left-aligned label that fills its cell up to the field (the Preferences style): its bounds end 6 px before the field.
    return not (a.text_align or "").endswith(("Right", "Center")) and 0 <= b.x - (a.x + a.w) <= 8


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
    tall_hi = None  # rows: the bottom of a tall list or grid in the group; a row that starts above it is beside it (no tolerance)
    for i in key:
        if cur and lo(i) >= cur_hi - tol and (tall_hi is None or lo(i) >= tall_hi):
            groups.append(cur)
            cur = []
            cur_hi = None
            tall_hi = None
        cur.append(i)
        cur_hi = hi(i) if cur_hi is None else max(cur_hi, hi(i))
        if axis == "y" and getattr(i, "kind", "") == "other" and i.h >= 120:
            tall_hi = hi(i) if tall_hi is None else max(tall_hi, hi(i))
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
        # Columns that hold containers (a column of fields and group boxes beside a column of group boxes) are laid out as columns: cut
        # into rows first, a tall box on one side would make each row as tall as it and open a band of empty space on the other side.
        cols = split(items, "x")
        if (len(cols) > 1 and any(i.kind in ("group", "panel", "tabs") or (i.kind == "other" and i.h >= 240) for i in items)
                and sum(1 for c in cols if len(split(c, "y", tol=5)) > 1) >= 1
                and all(not (len(c) == 1 and c[0].kind == "label") for c in cols)):
            return Block("h", items, [cut(g) for g in cols])
        return Block("v", items, [cut(g) for g in rows])
    cols = split(items, "x")
    if len(cols) > 1:
        return row_or_columns(items, cols)
    # A row whose items overlap a little vertically but not side by side: try the columns with a looser tolerance.
    cols = split(items, "x", tol=6)
    if len(cols) > 1:
        return row_or_columns(items, cols)
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


def row_or_columns(items, cols):
    """Blocks side by side; when each is one item or one row of items (a label and its field, then the field's button), they are one row,
    so what follows a field on its row is known when the field is sized."""
    subs = [cut(g) for g in cols]
    if all(b.kind == "leaf" or (b.kind == "h" and not b.relaxed and all(c.kind == "leaf" for c in b.children)) for b in subs):
        leaves = []
        for b in subs:
            leaves.extend([b] if b.kind == "leaf" else b.children)
        return Block("h", items, sorted(leaves, key=lambda c: c.x))
    return Block("h", items, subs)


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
        if has_date_box(m):
            return ink_x(m)[0]  # a date picker's box
        return m.x


def make_units(items, row_index):
    units = []
    i = 0
    while i < len(items):
        it = items[i]
        nxt = items[i + 1] if i + 1 < len(items) else None
        if (it.kind == "label" and nxt is not None and nxt.kind in ("field", "other", "button", "panel", "stacked") and nxt.x - (it.x + it.w) <= 40
                and (nxt.h <= 3 * GRID["field_height"] or abs(it.y - nxt.y) <= 8) and not it.fixed and not nxt.fixed):
            units.append(Unit([it, nxt], row_index))
            i += 2
            continue
        if (it.kind == "button" and nxt is not None and nxt.kind == "other" and 0 <= nxt.x - (it.x + it.w) <= 40 and abs(it.y - nxt.y) <= 4
                and it.h <= GRID["field_height"] + 4 and nxt.h > it.h + 6 and not it.fixed and not nxt.fixed):
            units.append(Unit([it, nxt], row_index))  # a button that names the box beside it (FormApptEdit's Lab and Req)
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
            # The clinic picker draws its box over its whole height: the standard field height like every combo box (UI-12 fix round; it
            # kept the designer's 21 px). The date picker lays its own box out inside its bounds.
            if "ODDatePicker" in n.type:
                return n.nh
            return H
        if k == "button" and in_field_row and n.h <= H + 6:
            return H
        if k in ("check", "radio"):
            return max(n.nh if n.h > self.p.check_height + 6 else 0, self.p.check_height)
        if k == "label" and n.text_w and n.text_h and n.text_w + 4 > n.nw and n.nh >= 1.6 * n.text_h:
            # A wrapped label keeps the height of its lines, and no more (UI-12 fix round: no empty lines under a note's text).
            room = max(20, n.nw - 4)
            need = max(note_lines(n.text, n.text_w, room), wrapped_lines(n.text_w, room)) * n.text_h + 2
            if need + 4 < n.nh and n.editable and not getattr(n, "keep_size", False):
                return need
            return n.nh
        if k == "label" and in_field_row and n.h <= H + 4:
            return H
        return n.nh

    def set_size(self, n, w, h, rule):
        if n.fixed or not n.editable or getattr(n, "keep_size", False):
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
        # Columns of containers only, the same number in each: the i-th containers of the columns form a row, with one top and one height.
        cols = []
        for k in kids:
            its = sorted(k.items, key=lambda i: i.ny)
            if not its or any(getattr(i, "kind", "") not in ("group", "panel") or i.fixed or not i.editable for i in its):
                cols = []
                break
            cols.append(its)
        if len(cols) >= 2 and len({len(c) for c in cols}) == 1:
            top = min(c[0].ny for c in cols)
            for row in zip(*cols):
                hmax = max(n.nh for n in row)
                for n in row:
                    self.set_loc(n, n.nx, top, "containers in a row across columns: one top")
                    self.set_size(n, n.nw, hmax, "containers in a row across columns: one height")
                top += hmax + GRID["sibling_gap"]
            return max(rights), top - GRID["sibling_gap"]
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
        # A check box or radio button hidden in the designer on a row of shown controls, taller than they are (FormPatientEdit's "Same
        # for entire super family" beside "Same for entire family", two lines tall), leaves its row and goes to the end of the column like
        # a hidden row (round 3: it left a band under the shown box).
        extra = []
        split_b = {}  # a row that lost its hidden check box -> the row's bottom with it (the gap under the row is measured from there)
        for idx, k in enumerate(kids):
            if k.kind != "h" or not is_row(k):
                continue
            its = row_items(k)
            shown_h = max([i.h for i in its if getattr(i, "visible", True)] + [0])
            hid = [i for i in its if not getattr(i, "visible", True) and getattr(i, "kind", "") in ("check", "radio") and i.h > shown_h + 4]
            if hid and shown_h:
                rest = [c for c in sorted(k.children, key=lambda c: c.x) if c.items[0] not in hid]
                kids[idx] = rest[0] if len(rest) == 1 else Block("h", [c.items[0] for c in rest], rest)
                split_b[id(kids[idx])] = k.b
                extra.extend(Block("leaf", [i]) for i in hid)
        kids = kids + extra
        # A row of controls hidden in the designer (shown at run time in some setups only, such as Canada's Time Units) goes to the end of
        # its column: where it stays hidden it leaves no gap between the rows that show.
        hidden = [k for k in kids if is_row(k) and all(not getattr(i, "visible", True) for i in row_items(k))]
        gap_over = {}
        if hidden and len(hidden) < len(kids):
            # The rows around a hidden row keep the gap they had to it, not the gap across it.
            for i in range(1, len(kids)):
                if kids[i] in hidden or kids[i - 1] not in hidden:
                    continue
                j = i - 1
                while j > 0 and kids[j] in hidden:
                    j -= 1
                if kids[j] not in hidden:
                    gap_over[id(kids[i])] = min(kids[j + 1].y - kids[j].b, kids[i].y - kids[i - 1].b)
            kids = [k for k in kids if k not in hidden] + hidden
        for i in range(1, len(kids)):
            if id(kids[i - 1]) in split_b and id(kids[i]) not in gap_over:
                gap_over[id(kids[i])] = kids[i].y - split_b[id(kids[i - 1])]
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
        # The stack's usual gap: a gap is a section break only when it is clearly larger than that (a form drawn loose all over is not
        # made of sections).
        gaps = sorted(kids[i + 1].y - kids[i].b for i in range(len(kids) - 1))
        typical = gaps[len(gaps) // 2] if gaps else 0
        bottom = y0
        rmax = x0
        for ri, k in enumerate(kids):
            if prev is not None:
                y += self.gap(prev, k, prev_kind, typical, gap_over.get(id(k)))
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
        # Notes and their buttons standing one per row in a column (under a list: "View all fee changes" over its Audit Trail button) are
        # centred on the column: a note over its button, not ragged against it.
        singles = [row_items(k)[0] if is_row(k) and len(row_items(k)) == 1 else None for k in kids]
        i = 0
        while i < len(singles):
            j = i
            while j < len(singles) and singles[j] is not None and singles[j].kind in ("label", "button") and singles[j].editable and not singles[j].fixed:
                j += 1
            run = singles[i:j]
            if len(run) >= 3 and any(n.kind == "label" for n in run) and sum(1 for n in run if n.kind == "button") >= 2:
                lo = min(min(n.nx for n in run), min((k.x for k in kids), default=x0))
                hi = max(max(n.nx + n.nw for n in run), rmax)
                axis = (lo + hi) // 2
                for n in run:
                    self.set_loc(n, axis - n.nw // 2, n.ny, "notes and their buttons: centred on their column")
                    if n.kind == "label" and not (n.text_align or "").endswith("Center"):
                        n.new_text_align = ("Top" if (n.text_align or "").startswith("Top") else "Middle") + "Center"
                        n.add_rule("TextAlign", "notes and their buttons: centred on their column")
            i = max(j, i + 1)
        # A wide list or grid alone on its row that ends a little short of the column's right edge reaches it (the grid under two columns of
        # fields and groups ends where the groups end).
        for k in kids:
            if is_row(k) and len(row_items(k)) == 1:
                g = row_items(k)[0]
                edges = [i.nx + i.nw for i in block.items if i is not g and i.kind in ("group", "panel", "tabs", "other", "field")]
                edge = max(edges) if edges else 0
                if (g.kind == "other" and g.nw >= 300 and g.editable and not g.fixed and not getattr(g, "members", None)
                        and not g.anchored_right_only and 0 < edge - (g.nx + g.nw) <= 48):
                    self.set_size(g, edge - g.nx, g.nh, "wide list: reaches the column's right edge")
        # A row of buttons only right under a row of lists ends where the last list ends (FormPatientEdit's Add and Defaults under the
        # Family Relationships list).
        for i in range(1, len(kids)):
            if not (is_row(kids[i]) and is_row(kids[i - 1])):
                continue
            bts = row_items(kids[i])
            above = row_items(kids[i - 1])
            if (len(above) >= 2 and all(a.kind == "other" and a.nh > 2 * self.p.field_height for a in above)
                    and all(b.kind == "button" and b.editable and not b.fixed for b in bts)):
                edge = max(a.nx + a.nw for a in above)
                dx = edge - max(b.nx + b.nw for b in bts)
                if dx and min(b.nx for b in bts) + dx >= min(a.nx for a in above):
                    for b in bts:
                        self.set_loc(b, b.nx + dx, b.ny, "buttons under a row of lists: end where the last list ends")
        # A row of captions right over a row of lists (as many): each caption starts where its list starts (FormPatientEdit's Position and
        # Family Relationships stood 19 px right of their lists).
        for i in range(1, len(kids)):
            if not (is_row(kids[i]) and is_row(kids[i - 1])):
                continue
            caps = sorted(row_items(kids[i - 1]), key=lambda n: n.nx)
            lsts = sorted(row_items(kids[i]), key=lambda n: n.nx)
            if (len(caps) >= 2 and len(caps) == len(lsts) and all(c.kind == "label" and c.editable and not c.fixed for c in caps)
                    and all(n.kind == "other" and n.nh > 2 * self.p.field_height for n in lsts)
                    and all(not (c.text_align or "").endswith(("Right", "Center")) for c in caps)):
                for c, n in zip(caps, lsts):
                    self.set_loc(c, n.nx, c.ny, "caption over its list: starts where the list starts")
                    w = max(n.nw, label_width(c))
                    if w != c.nw:
                        self.set_size(c, w, c.nh, "caption over its list: as wide as the list")
        # Rows of one column that end in a field end at one right edge (round 3: FormPractice's City, ST, Zip rows ended 10 px right of the
        # Address fields above them): the last field of a row within edge_snap of the column's most common right edge takes it.
        lasts = []
        for k in kids:
            if is_row(k):
                its = [i for i in row_items(k) if getattr(i, "visible", True)]
                f = its[-1] if its else None
                if (f is not None and (f.kind == "field" or (f.kind == "other" and "TextBox" in f.type and f.nw >= GRID["wide_field"]))
                        and f.editable and not f.fixed and not getattr(f, "members", None)
                        and not has_date_box(f) and not getattr(f, "keep_size", False)):
                    lasts.append(f)
        if len(lasts) >= 2:
            edges = collections.Counter(f.nx + f.nw for f in lasts)
            # The column's edges: its most common one and its widest (no chains of near edges).
            targets = {max(edges, key=lambda e: (edges[e], e)), max(edges)}
            for f in lasts:
                fe = f.nx + f.nw
                # Polish round: a wide field (wide_field or wider: FormScreenEdit's Grade Level, FormProvEdit's Taxonomy Code Override) up to
                # edge_grow short of the column's edge grows to it; short fields (numbers, codes) keep their width.
                grow = GRID["edge_grow"] if f.nw >= GRID["wide_field"] else GRID["edge_snap"]
                near = [e for e in targets if abs(e - fe) <= GRID["edge_snap"] or 0 < e - fe <= grow]
                if not near:
                    continue
                edge = max(near, key=lambda e: (edges[e], e))
                d = edge - fe
                if d and f.nw + d >= 24:
                    self.set_size(f, f.nw + d, f.nh, "fields of one column: one right edge")
                    rmax = max(rmax, edge)
        # Rows of a stack that are a list and a part beside it (FormPractice's right column: the Default Provider box beside one list,
        # the Billing Provider group beside the next): the parts beside the lists start at one x, the leftmost one's.
        seconds = []
        for k in kids:
            if k.kind == "h" and len(k.children) == 2:
                first, second = sorted(k.children, key=lambda c: c.x)
                if (first.kind == "leaf" and first.items[0].kind == "other" and first.items[0].nh > 2 * self.p.field_height
                        and all(i.editable and not i.fixed and not getattr(i, "members", None) for i in second.items)):
                    seconds.append(second)
        if len(seconds) >= 2:
            x_min = min(min(i.nx for i in sb.items) for sb in seconds)
            for sb in seconds:
                dx = x_min - min(i.nx for i in sb.items)
                if 0 > dx >= -GRID["edge_snap"]:
                    for i in sb.items:
                        self.set_loc(i, i.nx + dx, i.ny, "parts beside the lists of a column: one left edge")
        # The rightmost container or wide field of each row of a stack ends at the stack's right edge when it is within edge_snap of it
        # (FormPractice's Default Provider box, Billing Provider group and Switzerland group ended at three x values).
        ends = []
        for k in kids:
            its = [i for i in k.items if getattr(i, "visible", True)]
            if not its:
                continue
            m = max(its, key=lambda i: i.nx + i.nw)
            # A field counts when a caption above names it (no label beside it on its row).
            captioned = m.kind == "field" and not any(i is not m and abs(i.ny - m.ny) <= 4 and i.nx < m.nx for i in its)
            if m.kind in ("group", "panel") or (captioned and m.nw >= GRID["wide_field"] and not has_date_box(m)):
                if m.editable and not m.fixed and not getattr(m, "members", None) and not m.anchored_right_only:
                    ends.append(m)
        if len(ends) >= 3 and any(m.kind in ("group", "panel") for m in ends):
            edge = max(m.nx + m.nw for m in ends)
            for m in ends:
                d = edge - (m.nx + m.nw)
                if 0 < d <= 2 * GRID["edge_snap"]:
                    self.set_size(m, m.nw + d, m.nh, "the rightmost box of each row of a column: one right edge")
        # Containers stacked in one column (one left edge) share one right edge: the widest one's.
        conts = []
        for k in kids:
            if is_row(k):
                its = row_items(k)
                if (len(its) == 1 and its[0].kind in ("group", "panel", "tabs") and its[0].editable and not its[0].fixed
                        and not getattr(its[0], "members", None) and not its[0].anchored_right_only):
                    conts.append(its[0])
        for c0 in conts:
            col = [c for c in conts if abs(c.nx - c0.nx) <= 8]
            if len(col) < 2:
                continue
            r = max(c.nx + c.nw for c in col)
            for c in col:
                if c.nx + c.nw < r and c.nx == c0.nx:
                    self.set_size(c, r - c.nx, c.nh, "containers stacked in one column: one right edge")
            rmax = max(rmax, r)
        return rmax, bottom

    def gap(self, prev, cur, prev_kind, typical=0, over=None):
        orig = cur.y - prev.b if over is None else over
        pk = prev_kind
        ck = self.row_kind(cur)
        # Rows that both start with a check box in one column are one list of check boxes (a field or note further along a row does
        # not make a section): the list's gap.
        if is_row(prev) and is_row(cur) and orig < 3 * GRID["section_gap"]:
            a, b = row_items(prev)[0], row_items(cur)[0]
            if a.kind in ("check", "radio") and b.kind in ("check", "radio") and abs(a.x - b.x) <= 4:
                return GRID["list_gap"]
        if orig >= GRID["section_from"] and orig >= typical + 6 and (over is not None or is_separator(prev) or is_separator(cur)):
            # UI-12 fix round 2: a wider gap stays only where something explains it: a row hidden in the designer stood in it (its rows
            # keep the gap they had to it), or a visible separator line is one of the two rows. Otherwise it was main's loose spacing
            # (UserControlMainWindowMisc's 12 px between two groups and between two fields), and the rows take the gap of their kind.
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
        # Label + field columns that never share a row are one column: the fields of a stack start at one x (a row's own field that was
        # drawn further right joins the others), so a group reads as one form. Columns that share a row (two bands) stay apart.
        merged = True
        while merged:
            merged = False
            pair_cols = [c for c in cols if any(u.label is not None for u in c)]
            for i in range(len(pair_cols)):
                for j in range(i + 1, len(pair_cols)):
                    a, b = pair_cols[i], pair_cols[j]
                    if {u.row for u in a} & {u.row for u in b}:
                        continue
                    # Only when nothing else of the stack sits between them on any of their rows.
                    lo = min(min(u.anchor for u in a), min(u.anchor for u in b))
                    hi = max(max(u.anchor for u in a), max(u.anchor for u in b))
                    rows_ab = {u.row for u in a} | {u.row for u in b}
                    if any(lo < v.anchor < hi and v.row in rows_ab for v in others if v not in a and v not in b):
                        continue
                    a.extend(b)
                    cols.remove(b)
                    merged = True
                    break
                if merged:
                    break
        field_cols = [c for c in cols if any(u.label is not None for u in c)]
        prefs = self.prefs_x() is not None
        # Check boxes: at the first field column when every one fits there, else at the content's left edge (a check box list). In the
        # Preferences style the boxes are at the content's left edge and the labels start where their text starts.
        if checks and prefs:
            for u in checks:
                u.col = {"x": x0, "units": [u]}
        elif checks:
            target = None
            if stack_has_pairs and field_cols:
                fc = max(field_cols, key=lambda c: (sum(1 for u in c if u.label is not None), -min(u.anchor for u in c)))
                fx = self.hinted_x(fc, self.column_x(fc, x0))
                if all(fx + check_width(u.main) <= right for u in checks):
                    target = fc
            for u in checks:
                if target is not None:
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
        first_pair = next((c for c in cols if any(u.label is not None for u in c)), None)
        stack_id = len(self.p.col_records)
        for c in cols:
            nat = self.column_x(c, x0)
            col = {"x": nat, "units": c}
            if any(u.label is not None for u in c):
                if prefs and c is first_pair:
                    # The Preferences style: one field column for the whole page.
                    col["x"] = max(self.prefs_x(), x0 + 1)
                    col["prefs"] = True
                elif not prefs:
                    col["x"] = self.hinted_x(c, nat)
                    self.p.col_records.append((stack_id, abs_x(self.c) + nat, [u.main.name for u in c if u.label is not None]))
            elif (not prefs and first_pair is None and nat >= x0 + 40
                  and all(u.main.kind in ("field", "other", "check", "radio", "stacked") for u in c)):
                # A column of fields or check boxes without labels, indented to where the fields of the container beside it start (a
                # group of a note box and its check box): it joins that field column.
                col["x"] = self.hinted_x(c, nat, all_units=True)
                self.p.col_records.append((stack_id, abs_x(self.c) + nat, [u.main.name for u in c]))
            for u in c:
                u.col = col

    def prefs_x(self):
        """The page's field column in this container's coordinates (Preferences style), or None."""
        if self.p.prefs_F is None:
            return None
        return self.p.prefs_F - abs_x(self.c)

    def hinted_x(self, c, nat, all_units=False):
        """A field column's x: its own, or further right where the same column of another container (window coordinates) is."""
        base = abs_x(self.c)
        hints = [self.p.hints[u.main.name] - base for u in c if (u.label is not None or all_units) and u.main.name in self.p.hints]
        return max([nat] + hints)

    def column_x(self, c, x0):
        need = x0
        for u in c:
            if u.label is not None:
                need = max(need, x0 + pair_label_width(u.label) + GRID["label_gap"])
        # Check boxes that join a field column do not pull it left (their box goes where the fields are).
        anchors = [u.anchor for u in c if u.main.kind not in ("check", "radio")]
        if not anchors:
            anchors = [u.anchor for u in c if u.main.check_align != "MiddleRight"]
        orig = min(anchors) if anchors else need
        if orig - x0 < 4:
            orig = x0
        return max(need, orig)

    def prefs_label(self, u):
        """Preferences style: the label starts where the check box text starts and fills its cell up to 6 px before the field; a label
        longer than the cell wraps onto more lines (left-aligned, centred on its field)."""
        lb = u.label
        lx = self.left + GRID["label_lead"]
        lw = max(24, u.col["x"] - GRID["label_gap"] - lx)
        # UI-12 fix round 2: a label that wraps beside its field is as tall as its measured lines (measured_lines: word by word, as
        # the label draws them; the width estimate made UserControlEnterpriseGeneral's two-line label three lines tall, 12 px off its
        # field's centre), and no line starts with a hyphen (the label narrows until the hyphenated word moves down whole).
        lines = 1 if lb.text_w + 4 <= lw else measured_lines(lb.text, lb.text_w, lw - 2)
        if lines > 1:
            lw2 = hyphen_safe_width(lb.text, lb.text_w, lw)
            if lw2 < lw:
                lw = lw2
                lines = measured_lines(lb.text, lb.text_w, lw - 2)
        lh = self.p.field_height if lines == 1 else lines * (lb.text_h or 16) + 2
        self.set_size(lb, lw, lh, "label: Preferences style, from the check box text to 6 px before its field")
        if lb.autosize:
            lb.autosize_off = True
        if lb.text_align != "MiddleLeft":
            lb.new_text_align = "MiddleLeft"
            lb.add_rule("TextAlign", "label: Preferences style, left-aligned where the check box text starts")
        return lx

    def place_row(self, units, y, right):
        H = self.p.field_height
        field_row = any(u.main.kind == "field" or u.label is not None for u in units)
        prefs_lx = {}
        for u in units:
            if u.label is not None and u.col and u.col.get("prefs") and not u.label.fixed:
                prefs_lx[id(u)] = self.prefs_label(u)
        h_row = 0
        any_shown = any(getattr(it, "visible", True) for u in units for it in u.items)
        for u in units:
            for it in u.items:
                if not getattr(it, "visible", True) and any_shown:
                    # A control hidden in the designer beside shown ones (shown in some setups) does not make the row taller.
                    h_row = max(h_row, min(self.item_height(it, field_row), H))
                    continue
                if it.kind in ("check", "radio") and it.text_w and not it.fixed and not getattr(it, "members", None) and check_width(it) <= right - min(it.nx, u.col["x"] if u.col else it.nx) + GRID["check_slack"]:
                    h_row = max(h_row, self.p.check_height)  # a check box whose text fits on one line is one line tall
                    continue
                h_row = max(h_row, it.nh if (u.label is it and id(u) in prefs_lx) else self.item_height(it, field_row))
        if not units:
            return right, y, "fields"
        # Horizontal: left to right, each unit at its column's x, never before the previous unit's right + gap.
        cursor = None
        rmax = 0
        prev_orig_right = None
        ordered = sorted(units, key=lambda u: u.anchor)
        # The width the items that follow a unit on the row need (its button, its note): a field leaves them room before the content's edge.
        tails = []
        for idx in range(len(ordered)):
            tail = 0
            prev_r = max(sum(ink_x(i)) for i in ordered[idx].items)
            for v in ordered[idx + 1:]:
                if v.label is not None or min(ink_x(i)[0] for i in v.items) - prev_r > 40:
                    break
                tail += GRID["item_gap"] + sum(min(i.nw, check_width(i)) if i.kind in ("check", "radio") else i.nw for i in v.items)
                prev_r = max(sum(ink_x(i)) for i in v.items)
            tails.append(tail)
        for idx, u in enumerate(ordered):
            x = u.col["x"] if u.col else u.anchor
            orig_left = min(ink_x(i)[0] for i in u.items)
            if cursor is not None and u.label is None and prev_orig_right is not None and orig_left - prev_orig_right <= 40:
                # An item that belongs to the one before it (a field's button or unit label, a check box's note) follows it; a container
                # beside a container keeps the sibling gap.
                both = u.main.kind in ("group", "panel", "tabs") and ordered[idx - 1].main.kind in ("group", "panel", "tabs")
                x = cursor + (GRID["sibling_gap"] if both else GRID["item_gap"])
            elif cursor is not None and id(u) in prefs_lx:
                # Preferences style, a label after another control on its row (a radio button's text): it follows that control, and its
                # field moves right only when the label does not fit before the page's field column.
                lb = u.label
                lx = max(prefs_lx[id(u)], cursor + GRID["item_gap"])
                if x - GRID["label_gap"] - lx < lb.text_w + 4:
                    x = lx + lb.text_w + 4 + GRID["label_gap"]
                self.set_size(lb, x - GRID["label_gap"] - lx, self.p.field_height, "label: Preferences style, after the control before it")
                prefs_lx[id(u)] = lx
            elif cursor is not None:
                gap = GRID["item_gap"] if u.label is None else GRID["band_gap"]
                need = cursor + gap + (pair_label_width(u.label) + GRID["label_gap"] if u.label is not None else 0)
                if need > x:
                    x = need
                    if u.col and not u.col.get("prefs"):
                        u.col["x"] = max(u.col["x"], x)
            m = u.main
            # Check boxes: the box on the left.
            if m.kind in ("check", "radio") and m.text_w and not m.fixed and not m.members:
                if m.check_align == "MiddleRight":
                    m.new_check_align = "MiddleLeft"
                    m.add_rule("CheckAlign", "check box: the box on the left of its text")
                width = check_width(m)
                room = right - x
                if room < width <= room + GRID["check_slack"]:
                    width = room  # one line: the text fits as the box draws it
                hh = self.item_height(m, field_row) if width > room else self.p.check_height
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
            elif m.kind == "field" and not m.fixed and "ODDatePicker" in m.type:
                # The picker ends where its date box ends (the box keeps its own width; what follows on the row follows the box).
                if m.nw > DATE_BOX_X + DATE_BOX_W:
                    self.set_size(m, DATE_BOX_X + DATE_BOX_W, m.nh, "date picker: its bounds end with its date box")
            elif m.kind == "field" and not m.fixed:
                off_box = ink_x(m)[0] - m.x  # a clinic picker's box starts after its own label
                w0 = getattr(m, "w0", m.nw)
                w = (snap(w0 - off_box) if w0 - off_box > 8 else w0 - off_box)
                if abs(x + (w0 - off_box) + tails[idx] - right) <= 2:
                    w = right - tails[idx] - x  # a field that ends at the content's edge stays there (no snap away from it)
                if (((u.col and u.col.get("prefs") and ("ComboBox" in m.type or w >= GRID["wide_field"]))
                     or (self.p.in_scope and (u.label is not None or "ComboBoxClinicPicker" in m.type) and not self.p.keep_width))
                        and "ODDatePicker" not in m.type and right - tails[idx] - x >= 60):
                    # Preferences style: combo boxes and wide fields end at the content's right edge (one right edge for the page).
                    w = right - tails[idx] - x
                    u.at_edge = True
                if x + w + tails[idx] > right and right - tails[idx] - x >= 60:
                    # A field never runs past its container's content: it ends at the content's right edge, less what follows it on the row
                    # (the container keeps its width).
                    w = right - tails[idx] - x
                self.set_size(m, w + off_box, self.item_height(m, True), "single-line field: the standard height, width on the 4 px grid")
            elif (m.kind == "other" and u.col and u.col.get("prefs") and not m.fixed and m.editable and not getattr(m, "members", None)
                  and right - tails[idx] - x >= 60 and (m.nw >= GRID["wide_field"] or x + m.nw + tails[idx] > right)):
                # Preferences style: a note box beside its label ends at the content's right edge like the combo boxes.
                self.set_size(m, right - tails[idx] - x, m.nh, "box beside its label: Preferences style, ends at the content's right edge")
            elif (m.kind == "other" and u.label is not None and u.label.kind == "label" and m.nh > 30 and m.nw >= 200
                  and x > m.x and not m.fixed and m.editable and not getattr(m, "members", None) and x - m.x < m.nw // 3):
                # A wide box (a note) whose field column moved right keeps its right edge: it lines up with what stands above it.
                self.set_size(m, m.nw - (x - m.x), m.nh, "wide box: keeps its right edge when its column moves right")
            elif (m.kind == "other" and u.label is not None and u.label.kind == "button" and self.p.in_scope and not self.p.keep_width
                  and not m.fixed and right - tails[idx] - x >= 60):
                # A box named by a button (Lab, Req) in a part laid out alone: it ends where the fields end.
                self.set_size(m, right - tails[idx] - x, m.nh, "box beside its button: ends where the fields end")
            elif m.kind == "button" and field_row and not m.fixed:
                self.set_size(m, m.nw, self.item_height(m, True), "button in a field row: the field height")
            elif m.kind == "label" and not m.fixed and u.label is None and m.text_w and m.text_w + 4 > m.nw and m.nh > (m.text_h or 16) + 6:
                # A note that wraps: as tall as its lines (a label whose last line was cut under the next row); UI-12 fix round: and no
                # taller, so the group's bottom padding under it is the 8 px of every group (a designer height kept empty lines under it).
                lines = wrapped_lines(m.text_w, max(20, m.nw - 4))
                need = lines * (m.text_h or 16) + 2
                # Shrinking takes the larger of the two estimates (the word wrap and the measured width over the room).
                fit = max(lines, note_lines(m.text, m.text_w, max(20, m.nw - 4))) * (m.text_h or 16) + 2
                if need > m.nh:
                    self.set_size(m, m.nw, need, "label: as tall as its wrapped lines")
                    h_row = max(h_row, need)
                elif fit + 4 < m.nh and m.editable and not getattr(m, "keep_size", False):
                    self.set_size(m, m.nw, fit, "label: as tall as its wrapped lines (no empty lines under the text)")
            elif m.kind == "label" and not m.fixed:
                if m.text_w and m.text_w + 4 > m.nw and m.nw < right - x:
                    self.set_size(m, min(label_width(m), right - x), max(m.nh, self.item_height(m, field_row)), "label: its text fits")
                    if m.autosize:
                        m.autosize_off = True
            if u.label is not None and id(u) in prefs_lx:
                lb = u.label
                # Polish round: beside a multi-line box (taller than a field) the label stands on the box's first line, as in the ordinary
                # forms (UserControlTreatPlanGeneral's Default Note stood in the middle of its 66 px box).
                tall = h_row > H + 2 and lb.nh < h_row - 2
                self.set_loc(lb, prefs_lx[id(u)], y if tall else y + (h_row - lb.nh) // 2,
                             "label: Preferences style, on its box's first line" if tall else "label: Preferences style, centred on its field")
            elif u.label is not None and u.label.kind == "button":
                # A button that names the box beside it (Lab, Req): it stands where a label would, keeping its size, at the box's top.
                lb = u.label
                self.set_loc(lb, x - GRID["label_gap"] - lb.nw, y, "button before its box: 6 px before it, at its top")
            elif u.label is not None:
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
            off = ink_x(m)[0] - m.x if has_date_box(m) else 0  # a date picker's box is inside its bounds
            if m.members:
                self.move_by(m, x - off - m.nx, my - m.ny, "stacked controls: moved together")
            else:
                self.set_loc(m, x - off, my, "row: on the grid")
            cursor = m.nx + m.nw
            if m.kind in ("check", "radio") and m.text_w:
                cursor = m.nx + min(m.nw, check_width(m))
            prev_orig_right = max(sum(ink_x(i)) for i in u.items)
            rmax = max(rmax, m.nx + m.nw)
        if self.p.keep_width and rmax > right:
            # A scope that keeps its width: the row's widest field gives up the overflow and what follows it moves left.
            over = rmax - right
            fields = sorted([u.main for u in units if u.main.kind == "field" and not u.main.fixed and not has_date_box(u.main)
                             and u.main.editable and not getattr(u.main, "members", None)], key=lambda n: -n.nw)
            if fields and fields[0].nw - over >= 48:
                f = fields[0]
                fx = f.nx
                self.set_size(f, f.nw - over, f.nh, "single-line field: shrinks to the room its row has (the part keeps its width)")
                for u in units:
                    for it in u.items:
                        if it is not f and it.nx > fx:
                            if getattr(it, "members", None):
                                self.move_by(it, -over, 0, "row: on the grid")
                            else:
                                self.set_loc(it, it.nx - over, it.ny, "row: on the grid")
                rmax -= over
        kinds = {u.main.kind for u in units}
        # Containers side by side in a row get one height.
        conts = [u.main for u in units if u.label is None and u.main.kind in ("group", "panel", "tabs")]
        if len(conts) >= 2 and len(conts) == len(units) and len({n.ny for n in conts}) == 1:
            hmax = max(n.nh for n in conts)
            for n in conts:
                if not (n.anchor & 2 and n.anchor & 1):
                    self.set_size(n, n.nw, hmax, "containers side by side: one height")
        # Lists side by side in a row (FormPatientEdit's Status, Gender, Position and Family Relationships) get one top and one height.
        lists = [u.main for u in units if u.label is None and u.main.kind == "other" and u.main.nh > 2 * H and u.main.editable
                 and not u.main.fixed and not getattr(u.main, "members", None)]
        if len(lists) >= 2 and len(lists) == len(units):
            hmax = max(n.nh for n in lists)
            for n in lists:
                if not (n.anchor & 2 and n.anchor & 1):
                    self.set_size(n, n.nw, hmax, "lists side by side: one height")
                self.set_loc(n, n.nx, y, "lists side by side: one top")
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
        self.window_simple = ""
        self.prefs_F = None      # Preferences style: the page's one field column (page coordinates), or None
        self.hints = {}          # field name -> the x (window coordinates) of the column it shares with other containers' columns
        self.col_records = []    # this pass's field columns: (stack, x in window coordinates, field names)
        self.scopes = {}         # window -> [(container path, control names or None, keep width)] (Data/grid-scope.tsv)
        self.keep_width = False  # a scope that keeps its width: fields shrink to the room, containers grow only down
        self.in_scope = False    # laying out a scope: its panels fit their content like group boxes

    def measure(self, tree):
        # A bordered single-line text box's own height at the program's font (a borderless box, 16 px, is not a field's height).
        hs = [n.pref_h for n in walk(tree) if n.type == "System.Windows.Forms.TextBox" and n.pref_h >= 20 and not n.multiline]
        if hs:
            self.field_height = collections.Counter(hs).most_common(1)[0][0]
        ths = [n.text_h for n in walk(tree) if n.kind == "check" and n.text_h]
        if ths:
            self.check_height = max(GRID["check_height"], collections.Counter(ths).most_common(1)[0][0] + 2)

    def plan_window(self, window, tree, designer, simple):
        self.window_simple = simple
        if tree is None:
            self.unplaced.append((window, "", "no control tree"))
            return
        for n in walk(tree):
            if n is not tree and n.name in designer.name_to_field and n.name not in designer.declared:
                n.name = designer.name_to_field[n.name]
        names = [n.name for n in walk(tree) if n.name]
        fixed = runtime_positioned(self.root, simple, [x for x in names if x in designer.declared])
        NO_LABEL_PICKERS.clear()
        NO_LABEL_PICKERS.update(designer.no_label)
        MULTI_PICKERS.clear()
        MULTI_PICKERS.update(designer.multi_pickers)
        MULTI_PICKERS.update(multi_pickers_in_code(self.root, simple))
        for n in walk(tree):
            if "ComboBoxClinicPicker" in n.type and n.name not in NO_LABEL_PICKERS and not n.text_w:
                # A tree of a build before slice 1 (GridDump wrote no label width for a clinic picker): its box was drawn after main's fixed
                # 37 px label area. The box keeps its drawn width (w0: the width the field rules start from); the planned tree carries the
                # label's text width as GridDump writes it now.
                n.w0 = n.w - 37 + clinic_label_w(n)
                n.text_w = clinic_label_w(n) - GRID["label_gap"]
        for n in walk(tree):
            if n.name in designer.rtl and n.kind in ("check", "radio") and n is not tree and not SIMULATE:
                n.rtl = True
                n.check_align = "MiddleRight"
        for n in walk(tree):
            n.editable = n is tree or n.name in designer.declared
            if n is not tree and designer.docked.get(n.name, "None") != "None":
                n.dock = designer.docked[n.name]  # LayoutManager lays docked controls out itself; the designer says what they are
            if n.name in fixed and fixed[n.name].startswith("size: "):
                n.keep_size = True
            elif n.name in fixed:
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
        if SIMULATE:
            compared = mismatched = 0
        if compared and mismatched > 0.3 * compared:
            self.unplaced.append((window, ".", "the control tree is not the build of the designer file (%d of %d locations differ): dump again" % (mismatched, compared)))
            for n in walk(tree):
                n.editable = False
            return
        # The rules run again on their own result until a pass changes nothing (at most MAX_PASSES): a cut of the laid-out window can
        # find other rows than the cut of the drawn one, so one pass is not yet a fixed point. The plan is the last pass against the drawn
        # window, so a plan made from the build of an applied plan is empty.
        nodes = list(walk(tree))
        orig = {id(n): (n.x, n.y, n.w, n.h, n.check_align, n.text_align, n.autosize) for n in nodes}
        rules = {id(n): collections.OrderedDict() for n in nodes}
        flags = {id(n): [None, None, False] for n in nodes}
        self.hints = {}
        self.prefs_F = None
        for npass in range(MAX_PASSES):
            unplaced_before = len(self.unplaced)
            self.col_records = []
            self.prefs_F = self.page_field_column(tree) if self.is_prefs_page() else None
            if window in self.scopes:
                self.layout_scoped(tree, self.scopes[window], window)
            else:
                self.layout_tree(tree, is_root=True, window=window)
            hints_before = self.hints
            self.hints = self.column_hints()
            changed = self.hints != hints_before  # columns that now share an edge are laid out again
            for n in nodes:
                for k, v in n.rule.items():
                    rules[id(n)].setdefault(k, v)
                fl = flags[id(n)]
                if n.new_check_align:
                    fl[0] = n.new_check_align
                if n.new_text_align:
                    fl[1] = n.new_text_align
                fl[2] = fl[2] or n.autosize_off
                new_state = (n.nx, n.ny, n.nw, n.nh, n.new_check_align or n.check_align, n.new_text_align or n.text_align)
                if new_state != (n.x, n.y, n.w, n.h, n.check_align, n.text_align):
                    changed = True
                    if os.environ.get("GRID_DEBUG") and npass >= MAX_PASSES - 2:
                        print("pass %d %s %s: %s -> %s" % (npass, window, n.name, (n.x, n.y, n.w, n.h, n.check_align, n.text_align), new_state))
                n.x, n.y, n.w, n.h = n.nx, n.ny, n.nw, n.nh
                n.check_align = new_state[4]
                n.text_align = new_state[5]
                n.new_check_align = None
                n.new_text_align = None
                n.autosize = n.autosize and not n.autosize_off
                n.rule = collections.OrderedDict()
                n.spanned = False
            if not changed:
                break
            if npass < MAX_PASSES - 1:
                del self.unplaced[unplaced_before:]
        else:
            self.unplaced.append((window, ".", "no fixed point after %d passes: the last pass is planned" % MAX_PASSES))
        for n in nodes:
            ox, oy, ow, oh, ocheck, otext, oauto = orig[id(n)]
            n.x, n.y, n.w, n.h = ox, oy, ow, oh
            n.new_check_align = flags[id(n)][0] if flags[id(n)][0] and flags[id(n)][0] != ocheck else None
            n.new_text_align = flags[id(n)][1] if flags[id(n)][1] and flags[id(n)][1] != otext else None
            n.check_align, n.text_align, n.autosize = ocheck, otext, oauto
            n.autosize_off = flags[id(n)][2]
            n.rule = rules[id(n)]
            if (n.nx, n.ny) != (n.x, n.y):
                n.rule.setdefault("Location", "row: on the grid")
            if (n.nw, n.nh) != (n.w, n.h):
                n.rule.setdefault("Size", "container: its content and padding")
            if n.new_check_align:
                n.rule.setdefault("CheckAlign", "check box: the box on the left of its text")
            if n.new_text_align:
                n.rule.setdefault("TextAlign", "label: right-aligned, centred on its field")
        # Every check box of a laid-out window has its box on the left, also where the grid keeps the container or the control as drawn
        # (its bounds stay; the text follows the box inside them).
        for n in walk(tree):
            if (n.kind in ("check", "radio") and n.editable and n.text_w and n.check_align == "MiddleRight" and not n.new_check_align
                    and n.dock in ("None", "")):
                n.new_check_align = "MiddleLeft"
                n.rule.setdefault("CheckAlign", "check box: the box on the left of its text (its bounds kept)")
                if n.fixed:
                    self.add(designer, window, n.name, "CheckAlign", n.check_align, "MiddleLeft", n.rule["CheckAlign"])
        # Rows of the plan.
        for n in walk(tree):
            if not n.editable or n.fixed:
                continue
            if n is not tree and n.dock not in ("None", ""):
                continue
            if n is tree:
                if (n.nw, n.nh) != (n.w, n.h):
                    prop = "ClientSize" if designer.size(".", "ClientSize") else "Size"
                    # The designer's size less the built root's (a window's menu bar takes its height from the client area).
                    sz = designer.size(".", prop) if not SIMULATE else None
                    dw, dh = (sz[0] - n.w, sz[1] - n.h) if sz else (0, 0)
                    if sz and (n.nw + dw, n.nh + dh) == sz:
                        continue
                    self.add(designer, window, ".", prop, "%d,%d" % ((sz[0], sz[1]) if sz else (n.w, n.h)), "%d,%d" % (n.nw + dw, n.nh + dh),
                             n.rule.get("Size", "window grows"))
                continue
            loc = designer.point(n.name) if not SIMULATE else (n.x, n.y)
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
        for n in walk(tree):
            if n.rtl and n.editable and n.dock in ("None", ""):
                self.add(designer, window, n.name, "RightToLeft", "Yes", "No", "check box: the box on the left of its text (not right to left)")

    def layout_scoped(self, tree, scopes, window):
        """A window the grid lays out in parts (Data/grid-scope.tsv): each scope is a container (its whole content) or named controls of a
        container (a region of a window whose other parts the code lays out or that stay as drawn). A scope keeps its width when asked
        (fields shrink to the room). When a part grows, what stands below it keeps its distance to it (it moves down), what stands right of
        it moves right, its container grows by as much (up to the window), and controls on the window's bottom edge stay on it (tall ones
        grow). Every distance is measured on the window as it was before this pass, so parts side by side push once, not twice."""
        by_path = {n.path: n for n in walk(tree)}
        self.orig = {id(n): (n.nx, n.ny, n.nw, n.nh) for n in walk(tree)}
        self.scope_moved = set()
        for path, names, keep in scopes:
            c = by_path.get(path)
            if c is None:
                self.unplaced.append((window, path, "scope: no such container"))
                continue
            self.keep_width = keep
            self.in_scope = True
            self.scope_root = c
            try:
                if names is None:
                    old_r = c.nx + c.nw
                    self.layout_tree(c, c is tree, window)
                    self.scope_moved.add(id(c))
                    if c is tree:
                        continue
                    ox, oy, ow, oh = self.orig[id(c)]
                    if c.nx + c.nw > old_r:
                        self.push_right(tree, c.parent, c.ny, c.ny + c.nh, old_r, c.nx + c.nw - old_r, {id(c)})
                    self.push_down(tree, c.parent, c.nx, c.nx + c.nw, oy + oh, c.ny + c.nh, {id(c)})
                else:
                    members = [k for k in c.children if k.name in names]
                    if not members:
                        self.unplaced.append((window, path, "scope: none of its controls found"))
                        continue
                    widened = []
                    for k in members:
                        if k.children and k.editable and not k.fixed and self.lays_out(k):
                            old_r = k.nx + k.nw
                            self.layout_tree(k, False, window)
                            if k.nx + k.nw > old_r:
                                widened.append((k, old_r, k.nx + k.nw - old_r))
                    # A container of the scope that grew wider pushes what stands right of it (not of the scope) to the right.
                    for k, old_r, dw in widened:
                        self.push_right(tree, c, k.ny, k.ny + k.nh, old_r, dw, {id(m) for m in members})
                    items = merge_stacked([k for k in members if k.dock in ("None", "") and k.w > 0 and k.h > 0])
                    # Where the scope stands now (a part laid out before it may have pushed it down or right).
                    ink = [ink_x_new(i) if not getattr(i, "members", None) else (i.nx + ink_x(i)[0] - i.x, ink_x(i)[1]) for i in items]
                    x0 = min(q[0] for q in ink)
                    x1 = max(q[0] + q[1] for q in ink)
                    y0 = min(i.ny for i in items)  # the cut reads the drawn places; the scope is placed where it stands now
                    old_b = max(self.orig[id(k)][1] + self.orig[id(k)][3] for k in members)
                    lay = Layout(self, c, (x0, y0, x1, max(i.y + i.h for i in items)))
                    lay.place(cut(items), x0, y0, x1)
                    for pr in lay.problems:
                        self.unplaced.append((window, c.name or ".", pr))
                    self.scope_moved.update(id(k) for k in members)
                    self.push_down(tree, c, min(k.nx for k in members), max(k.nx + k.nw for k in members), old_b,
                                   max(k.ny + k.nh for k in members), {id(k) for k in members})
            finally:
                self.keep_width = False
                self.in_scope = False
        # Group boxes laid out as scopes that stand side by side (one top, one parent) get one height.
        scoped = [by_path[path] for path, names, keep in scopes if names is None and path in by_path and by_path[path].kind == "group"]
        for c in scoped:
            row = [d for d in scoped if d.parent is c.parent and d.ny == c.ny]
            hmax = max(d.nh for d in row)
            if len(row) > 1 and c.nh < hmax and not (c.anchor & 1 and c.anchor & 2):
                Layout(self, c.parent, (0, 0, c.parent.nw, c.parent.nh)).set_size(c, c.nw, hmax, "containers side by side: one height")
        self.settle_bottom_edge(tree)

    def push_right(self, tree, p, y0, y1, old_r, dw, moved):
        """What stands right of a scope that grew wider moves right by dw; the container (up to the window) grows by dw."""
        lay = Layout(self, p, (0, 0, p.nw, p.nh))
        old_w = p.nw
        for s in p.children:
            if id(s) in moved or s.dock not in ("None", "") or s.fixed or not s.editable:
                continue
            if s.nx >= old_r - 4:
                lay.set_loc(s, s.nx + dw, s.ny, "scope: right of a part that grew, moved right with it")
        if p is tree:
            p.nw = old_w + dw
            p.add_rule("Size", "window grows to its content")
            return
        if p.editable and not p.fixed:
            pr = p.nx + p.nw
            p.nw = old_w + dw
            p.add_rule("Size", "container: its content and padding")
            if p.parent is not None:
                self.push_right(tree, p.parent, p.ny, p.ny + p.nh, pr, dw, {id(p)})

    def push_down(self, tree, p, x0, x1, orig_b, new_b, moved):
        """A part of container p that ended at orig_b (before this pass) now ends at new_b: the controls below it keep their distance to it,
        and p keeps the distance from that part to its own bottom edge (it grows, and pushes its own neighbours in turn)."""
        if new_b <= orig_b:
            return
        lay = Layout(self, p, (0, 0, p.nw, p.nh))
        for s in p.children:
            if id(s) in moved or s.dock not in ("None", "") or s.fixed or not s.editable:
                continue
            ox, oy, ow, oh = self.orig[id(s)]
            if oy >= orig_b - 4 and s.nx < x1 and s.nx + s.nw > x0:
                target = new_b + (oy - orig_b)
                if target > s.ny:
                    lay.set_loc(s, s.nx, target, "scope: below a part that grew, keeps its distance to it")
                    self.scope_moved.add(id(s))
                    self.push_down(tree, p, s.nx, s.nx + s.nw, oy + oh, s.ny + s.nh, {id(s)})
        ox, oy, ow, oh = self.orig[id(p)]
        need = new_b + max(0, oh - orig_b)
        if need > p.nh:
            if p is tree:
                p.nh = need
                p.add_rule("Size", "window grows to its content")
            elif p.editable and not p.fixed:
                p.nh = need
                p.add_rule("Size", "container: its content and padding")
                if p.parent is not None:
                    self.push_down(tree, p.parent, p.nx, p.nx + p.nw, oy + oh, p.ny + p.nh, {id(p)})

    def settle_bottom_edge(self, tree):
        """The window grew: controls that stood on its bottom edge (and were not moved by a part above them) stay on it; tall ones grow."""
        ox, oy, ow, oh = self.orig[id(tree)]
        dh = tree.nh - oh
        if dh <= 0:
            return
        lay = Layout(self, tree, (0, 0, tree.nw, tree.nh))
        for s in tree.children:
            if id(s) in self.scope_moved or s.dock not in ("None", "") or s.fixed or not s.editable:
                continue
            sx, sy, sw, sh = self.orig[id(s)]
            if abs(oh - (sy + sh)) <= 16:
                if sh >= 100:
                    lay.set_size(s, s.nw, sh + dh, "scope: the window grew; a control on its bottom edge grows with it")
                else:
                    lay.set_loc(s, s.nx, sy + dh, "scope: the window grew; a control on its bottom edge moves with it")

    def is_prefs_page(self):
        return self.window_simple.startswith("UserControl")

    def page_field_column(self, tree):
        """Preferences style: the page's one field column (page coordinates): the widest label + 6 px after the check box text of its
        container, within label_cap of the page's content width (a longer label wraps)."""
        needs = []
        for c in walk(tree):
            if c.kind == "tabs" or not self.lays_out(c):
                continue
            left = self.content_rect(c, c is tree)[0]
            kids = [k for k in c.children if k.dock in ("None", "") and k.visible and k.w > 0 and k.h > 0 and not k.in_layout_panel]
            for lb in kids:
                if lb.kind != "label" or not lb.text_w or not lb.editable or lb.fixed:
                    continue
                if paired_field(lb, kids) is None:
                    continue
                needs.append(abs_x(c) + left + GRID["label_lead"] + lb.text_w + 4)
        if not needs:
            return None
        pl, _, pr, _ = self.content_rect(tree, True)
        cap = pl + GRID["label_cap"] * (pr - pl)
        return snap_up(min(cap, max(needs)) + GRID["label_gap"])

    def column_hints(self):
        """Field columns of different containers (or stacks) whose x in window coordinates are within hint_snap share the rightmost x: the
        fields of one column of the window start at one left edge across group boxes, tab pages and panels."""
        recs = sorted(self.col_records, key=lambda r: r[1])
        hints = {}
        i = 0
        while i < len(recs):
            cluster = [recs[i]]
            j = i + 1
            while j < len(recs) and recs[j][1] - recs[i][1] <= GRID["hint_snap"]:
                if all(r[0] != recs[j][0] for r in cluster):
                    cluster.append(recs[j])
                j += 1
            if len(cluster) > 1:
                target = max(r[1] for r in cluster)
                for r in cluster:
                    for name in r[2]:
                        hints[name] = max(hints.get(name, target), target)
            i += 1
        return hints

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
        scroll = 0
        if is_root and self.window_simple.startswith("UserControl") and n.nh > PREFS_PAGE_HEIGHT:
            # A preference page taller than FormPreferences' page area scrolls (FormPreferences sets AutoScroll): its vertical scroll bar
            # takes this much of the width, and the content keeps 8 px from it.
            scroll = GRID["scroll_bar"]
        if n.kind == "group":
            top = (n.title_bottom + GRID["title_gap"]) if n.title_bottom else pad
        right = n.nw - pad - scroll
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
                # On a Preferences page a group box alone on its rows spans the page too (the Claim Identification Prefix group stood in
                # the right half of the page, a blank half beside it).
                alone = (self.is_prefs_page() and is_root and c.kind == "group" and c.visible
                         and not any(o is not c and o.visible and o.dock in ("None", "") and min(o.y + o.h, c.y + c.h) - max(o.y, c.y) > 2
                                     for o in n.children))
                if (c.kind in ("group", "panel") and c.editable and not c.fixed and c.dock in ("None", "")
                        and (alone or (abs(c.x - left) <= GRID["span_snap"] and abs(c.x + c.w - right) <= GRID["span_snap"]))
                        and not (c.anchor & 8 and not c.anchor & 4)):
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
        # Controls with no size (a label kept only for its tooltip text) take no row: they stay as drawn.
        movable = [c for c in n.children if c.dock in ("None", "") and not c.in_layout_panel and c.w > 0 and c.h > 0]
        if not movable:
            return
        # A check box drawn taller than its one line of text (its text fits its width) is one line tall: it takes one row.
        lay0 = Layout(self, n, (0, 0, n.nw, n.nh))
        for c in movable:
            if (c.kind in ("check", "radio") and c.text_w and c.editable and not c.fixed and c.nh > self.check_height + 6
                    and check_width(c) <= c.nw and chr(10) not in c.text):
                lay0.set_size(c, c.nw, self.check_height, "check box: one line tall, its text fits")
                c.h = c.nh  # the cut reads it at its one-line height
        items = merge_stacked(movable)
        left, top, right, bottom = self.content_rect(n, is_root)
        lay = Layout(self, n, (left, top, right, bottom))
        bottom_items = []
        if is_root:
            bottom_items = [i for i in items if (i.anchored_bottom_only or (i.name in DIALOG_NAMES and i.kind == "button"))
                            and i.y + i.h > n.h - 60]
            # Buttons on the row of the bottom bar's buttons belong to the bar (Delete, Print ... Save).
            if bottom_items:
                for i in list(items):
                    if (i.kind == "button" and i not in bottom_items
                            and any(min(i.y + i.h, b.y + b.h) - max(i.y, b.y) > 4 for b in bottom_items if b.kind == "button")):
                        bottom_items.append(i)
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
        # A part that keeps its width keeps it; the group boxes inside it grow to their content (round 3: FormClaimEdit's tab pages scroll,
        # their group boxes must not cut their fields).
        grow_w = n.nw if (self.keep_width and (n is getattr(self, "scope_root", None) or n.kind != "group")) else max(n.nw, need_w)
        if (n.kind == "group" or (n.kind == "panel" and self.in_scope)) and not is_root:
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
        dialog = sorted([i for i in items if i.name in DIALOG_NAMES and (i.anchor & 8 or not i.anchor & 4 or i.x > n.w // 2)], key=lambda i: i.x)
        others = [i for i in items if i not in dialog]
        # The bar's left part starts at the padding (a Delete button drawn a few px further in).
        left = [i for i in others if i.x < n.w // 2 and not i.anchored_right_only]
        dx = 0
        if left and pad < min(i.x for i in left) <= pad + 16:
            dx = pad - min(i.x for i in left)
        for i in others:
            if i.kind == "button" and not getattr(i, "members", None):
                # Buttons of the bar stand on one bottom line with the dialog buttons.
                lay.set_loc(i, i.nx + (dx if i in left else 0), n.nh - pad - i.nh, "bottom bar: its buttons 8 px from the window's bottom")
                continue
            lay.move_by(i, dx if i in left else 0, dy, "bottom bar: 8 px from the window's bottom")
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
    if has_date_box(a) or has_date_box(b):
        # A date picker's bounds hold an empty strip left of its date box (where its label sits): only the box counts.
        ax, aw = ink_x(a) if has_date_box(a) else (a.x, a.w)
        bx, bw = ink_x(b) if has_date_box(b) else (b.x, b.w)
        ix = max(0, min(ax + aw, bx + bw) - max(ax, bx))
        iy = max(0, min(a.y + a.h, b.y + b.h) - max(a.y, b.y))
        small = min(aw * a.h, bw * b.h)
        return ix * iy / float(small) if small > 0 else 0
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
        # Check boxes stacked on one place (one shown at a time) are laid out as a check box.
        kinds = {m.kind for m in members}
        self.kind = "check" if kinds == {"check"} else "stacked"
        if self.kind == "check":
            self.text_w = max(m.text_w for m in members)
            self.text_h = max(m.text_h for m in members)
            self.check_align = members[0].check_align
        self.multiline = False
        self.text_align = ""
        self.new_check_align = None
        self.new_text_align = None
        self.autosize_off = False

    @property
    def anchored_bottom_only(self):
        return bool(self.anchor & 2) and not (self.anchor & 1)

    @property
    def anchored_right_only(self):
        return bool(self.anchor & 8) and not (self.anchor & 4)

    def add_rule(self, prop, rule):
        pass


def has_date_box(n):
    """A date picker or a clinic picker with its own label, or controls stacked with one: the box is inside the bounds (ink_x)."""
    if getattr(n, "members", None):
        return any(has_date_box(m) for m in n.members)
    t = getattr(n, "type", "")
    return "ODDatePicker" in t or ("ComboBoxClinicPicker" in t and getattr(n, "name", "") not in NO_LABEL_PICKERS)


def abs_x(n):
    """The x of a control's origin in its window (the root's client area)."""
    x = 0
    while n is not None and n.parent is not None:
        x += n.nx
        n = n.parent
    return x


def paired_field(lb, kids):
    """The control a label names: on its row, starting within 40 px after the label's bounds (or 8 px into them)."""
    best = None
    for k in kids:
        if k is lb or k.kind not in ("field", "other", "button", "panel", "stacked") or not k.visible:
            continue
        if min(lb.y + lb.h, k.y + k.h) - max(lb.y, k.y) <= 2:
            continue
        gap = k.x - (lb.x + lb.w)
        if -8 <= gap <= 40 and abs((lb.y + lb.h / 2.0) - (k.y + k.h / 2.0)) <= max(8, k.h / 2.0) and (best is None or k.x < best.x):
            best = k
    return best


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


SIMULATE = False
# ComboBoxClinicPicker's own label area before its box, when ShowLabel: since slice 1 its measured text (as a Label measures it, Segoe UI 9)
# + the 6 px label gap (ComboBoxClinicPicker.WidthLabelArea, the lead's answer of 2026-11-22); main's fixed 37 cut "Clinics" at its left.
CLINIC_LABEL_W = {"Clinic": 43, "Clinics": 48}
NO_LABEL_PICKERS = set()  # this window's clinic pickers with ShowLabel = false
MULTI_PICKERS = set()  # this window's clinic pickers with IsMultiSelect = true (the label "Clinics")


def clinic_label_w(i):
    """The label area of a clinic picker (its box starts this far into its bounds)."""
    return CLINIC_LABEL_W["Clinics" if getattr(i, "name", "") in MULTI_PICKERS else "Clinic"]
DATE_BOX_X = 63   # ODDatePicker's date box: its x in the picker and its width at 96 dpi (ODDatePicker.OnSizeChanged)
DATE_BOX_W = 102
PREFS_PAGE_HEIGHT = 836  # FormPreferences' panelMain: the page area (UI-12 round 2: grown to the tallest page, a 872 px client area; the owner allows about 1000)
MAX_PASSES = 8


def write_tree(path, trees):
    """The planned trees in GridDump's format (bounds and alignments as planned): the input of a simulated second run (--simulate)."""
    cols = ["window", "path", "name", "type", "index", "x", "y", "width", "height", "client_x", "client_y", "client_width", "client_height", "visible",
            "dock", "anchor", "auto_size", "sizes_itself", "layout_panel_child", "check_align", "text_align", "multiline", "font_height",
            "text_width", "text_height", "title_bottom", "preferred_height", "text"]

    def clean(v):
        return v.replace("\\", "\\\\").replace("\t", "\\t").replace("\n", "\\n")
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\t".join(cols) + "\n")
        for w, t in trees.items():
            for n in walk(t):
                vals = [n.window, clean(n.path), clean(n.name), n.type, n.index, n.nx, n.ny, n.nw, n.nh, n.cx, n.cy, n.cw, n.ch, int(n.visible), n.dock,
                        n.anchor, int(n.autosize and not n.autosize_off), int(n.sizes_itself), int(n.in_layout_panel),
                        n.new_check_align or n.check_align, n.new_text_align or n.text_align, int(n.multiline), n.font_h, n.text_w, n.text_h,
                        n.title_bottom, n.pref_h, clean(n.text)]
                f.write("\t".join(str(v) for v in vals) + "\n")


def cmd_plan(args):
    global SIMULATE
    SIMULATE = args.simulate
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
    scope_file = os.path.join(root, args.scope_file)
    if os.path.exists(scope_file):
        for r in fit_tools.read_tsv(scope_file):
            names = [x.strip() for x in (r.get("controls") or "").split(",") if x.strip()]
            planner.scopes.setdefault(r["window"], []).append((r["container"], names or None, (r.get("keep") or "") == "width"))
    for w, tree in trees.items():
        mode, note = modes.get(w, ("tool", ""))
        # The window's code sizes the window itself (Width, Height, Size, ClientSize set on the form): listed, tool and hand windows alike,
        # so the slice checks each size against the new layout (the planner does not change code).
        if not SIMULATE:
            for line in runtime_window_sized(root, w.split(".")[-1]):
                planner.unplaced.append((w, ".", "the code sizes the window at run time (check the size against the new layout): " + line))
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
    if args.tree_out:
        write_tree(args.tree_out, trees)
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
    if prop == "RightToLeft":
        pat = re.compile(r"(%sRightToLeft ?= ?System\.Windows\.Forms\.RightToLeft\.)(\w+)(;)" % re.escape(target))
        m = pat.search(text)
        if m:
            return text[:m.start(2)] + new + text[m.end(2):], None
        return text, "no RightToLeft line"
    if prop == "BorderStyle":
        # A hand row only (FormPatientEdit's Zip text box drawn borderless inside its combo box).
        pat = re.compile(r"(%sBorderStyle ?= ?System\.Windows\.Forms\.BorderStyle\.)(\w+)(;)" % re.escape(target))
        m = pat.search(text)
        if m:
            return text[:m.start(2)] + new + text[m.end(2):], None
        if new == "Fixed3D":
            return text, None  # the default
        return insert_before_name_line(text, control, "this.%s.BorderStyle = System.Windows.Forms.BorderStyle.%s;" % (control, new))
    if prop == "AddBefore":
        # A hand row only (fix round: FormScreenEdit's age arrows in front of the Age box they stand in): the control's Controls.Add line moves
        # to just before the named sibling's, so it is added first and drawn in front of it. A second run finds it there and changes nothing.
        pat_c = re.compile(r"^([ \t]*)this\.(?:(\w+)\.)?Controls\.Add\(this\.%s\);(\r\n|\n)" % re.escape(control), re.M)
        mc = pat_c.search(text)
        if not mc:
            return text, "no Controls.Add line"
        parent = mc.group(2)
        pat_s = re.compile(r"^[ \t]*this\.%sControls\.Add\(this\.%s\);(\r\n|\n)" % (re.escape(parent + ".") if parent else "", re.escape(new)), re.M)
        ms = pat_s.search(text)
        if not ms:
            return text, "no Controls.Add line for " + new
        if mc.start() < ms.start():
            return text, None
        line = text[mc.start():mc.end()]
        text = text[:mc.start()] + text[mc.end():]
        ms = pat_s.search(text)
        return text[:ms.start()] + line + text[ms.start():], None
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
    a.add_argument("--scope-file", default="tests/UiRefreshTests/Data/grid-scope.tsv")
    a.add_argument("--out", required=True)
    a.add_argument("--unplaced")
    a.add_argument("--tree-out", help="write the planned trees (the input of --simulate)")
    a.add_argument("--simulate", action="store_true", help="plan a --tree-out file without designer checks: the idempotence check")
    b = sub.add_parser("apply")
    b.add_argument("--plan", required=True)
    args = p.parse_args()
    {"plan": cmd_plan, "apply": cmd_apply}[args.cmd](args)


if __name__ == "__main__":
    main()
