"""Common steps of the UI-12 slice 11 layout scripts (the large windows and the earlier skips), on top of slice 8's _s8.py (bar, size,
move, spread) and slice 6's _edges.py: the 12 px section gap over the bottom bar (UI-12.md section 1, the lead's decision for slice 11),
a label right-aligned before its field, and a group box's children moved with it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)

PAD = 8
SECTION = 12  # the section gap over the bottom bar
W = s.W
H = s.H
bar = s.bar
move = s.move
spread = s.spread


def label_before(h, label, field, gap=6):
    """The label right-aligned, its right edge gap px before the field, on the field's row (the field height)."""
    f = h.n(field)
    lw = h.lw(label)
    h.text_align(label, "MiddleRight")
    h.set(label, f.nx - gap - lw, f.ny, lw, h.H)


def shift_children(h, parent, dx=0, dy=0):
    """Moves every child of the container by dx, dy (relative to the container), as when the container moves by -dx, -dy."""
    for k in h.n(parent).children:
        if k.name:
            h.set(k.name, k.nx + dx, k.ny + dy)
