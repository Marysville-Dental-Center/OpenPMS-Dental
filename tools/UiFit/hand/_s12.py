"""Common steps of the UI-12 slice 12 layout scripts (the plain exes' windows and the graph's, on the planner's tree), on top of slice 8's
_s8.py (bar, move, spread, size) and slice 6's _edges.py: a column of label + field rows (the labels from the padding, right-aligned and 6
px before the fields), a label put 6 px before its field, and a row of side-by-side group boxes with one top and one height. A plain window's
tree root is its client area (grid_tools.plain_client_root), so W and H are the client size."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s8 = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s8)

PAD = 8
GAP = 8  # sibling gap, the gap between the buttons of a bar
LGAP = 6  # label to field
ROWGAP = 4  # between two field rows (the pitch is the field height + 4)
SECTION = 12  # the section gap, over the bottom bar
TITLE = 23  # a group box's first row: under the title band (18) and the 4 px under it, as the planner places it
CAPGAP = 2  # a caption over its box

W = s8.W
H = s8.H
bar = s8.bar
move = s8.move
spread = s8.spread
size = s8.size


def col_x(h, labels, x=PAD):
    """The field column's x for labels that start at x: the widest label's text + the label gap."""
    return x + max(h.lw(n) for n in labels) + LGAP


def row(h, label, field, y, fx, fw=None, lx=PAD, fh=None):
    """A label + field row at y: the label from lx, right-aligned, ending LGAP before the field at fx; the field fw wide (default its own)
    and the field height tall (fh: another height, the label centred on the field's first line). Returns the row's bottom."""
    hh = fh or h.H
    if label:
        h.set(label, lx, y, fx - LGAP - lx, h.H)
        h.text_align(label, "MiddleRight" if not fh else "TopRight")
    h.set(field, fx, y, fw, hh)
    return y + hh


def rows(h, pairs, y, fx, right=None, lx=PAD):
    """Label + field rows from y at the pitch; a field 120 px or wider (or None for every field when right is given as 'all') ends on
    right. Returns the bottom of the last row."""
    yb = y
    for label, field in pairs:
        f = h.n(field)
        fw = None
        if right is not None and (f.nw >= 120 or f.w >= 120):
            fw = right - fx
        yb = row(h, label, field, y, fx, fw, lx=lx)
        y = yb + ROWGAP
    return yb


def gap_before(h, label, field):
    """The label ends LGAP before its field (right-aligned, centred on the field): its left edge stays where it can, else it moves left."""
    l = h.n(label)
    f = h.n(field)
    need = h.lw(label)
    right = f.nx - LGAP
    x = l.nx if right - l.nx >= need else right - need
    h.set(label, x, f.ny, right - x, f.nh)
    h.text_align(label, "MiddleRight")


def note(h, name, x, y, w, estimate=True):
    """A wrapped note at x, y, w wide and as tall as its lines. Returns its bottom."""
    nh = h.note_h(name, w, estimate=estimate)
    h.set(name, x, y, w, nh)
    return y + nh


def same_row(h, names, top, height=None):
    """Side-by-side containers: one top and one height (the tallest, or height)."""
    hh = height or max(h.n(n).nh for n in names)
    for n in names:
        h.set(n, None, top, None, hh)
    return top + hh
