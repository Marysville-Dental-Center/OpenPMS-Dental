"""Common steps of the UI-12 slice 13 layout scripts (the windows the tools could not reach: the seven the harness now builds with sample
data, the report designer's dialogs and controls, ODR's controls), on top of slice 11's _s11.py (bar, move, spread, the 12 px section gap
over the bottom bar, label_before, shift_children): a label column of right-aligned labels before one field column, and controls stacked
down a column with one gap."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)

PAD = 8
GAP = 8
SECTION = s.SECTION
W = s.W
H = s.H
bar = s.bar
move = s.move
spread = s.spread
label_before = s.label_before
shift_children = s.shift_children


def label_column(h, rows, x, y=None, pitch=27, fw=None, fh=None, field_x=None):
    """rows: (label, field) pairs, one a row from y (each field's own y when None) every pitch px; the labels right-aligned in one column
    from x as wide as the widest, the fields at one left edge 6 px after it (or field_x), fw wide when given, fh tall (the field height
    when None and the field is a single-line field: pass fh=0 to keep its height). Returns the field column's x."""
    lw = max(h.lw(lb) for lb, _ in rows if lb)
    fx = field_x if field_x is not None else x + lw + 6
    for i, (lb, f) in enumerate(rows):
        fn = h.n(f)
        fy = fn.ny if y is None else y + i * pitch
        hh = None if fh == 0 else (fh or h.H)
        h.set(f, fx, fy, fw, hh)
        if lb:
            h.text_align(lb, "MiddleRight")
            h.set(lb, fx - 6 - lw, fy, lw, hh if hh else fn.nh)
    return fx


def stack(h, names, y, gap=GAP, x=None):
    """The controls one under another from y, gap px apart (their heights kept). Returns the bottom of the last."""
    for name in names:
        n = h.n(name)
        h.set(name, x, y)
        y += n.nh + gap
    return y - gap
