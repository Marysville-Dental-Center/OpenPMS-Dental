"""FormApptRules on the grid (UI-12 slice 7, layout script on the planner's tree). The note and the grid stood 21 px from the left and 33 px
from the right, the grid 38 px over Add: both on the content edges, the note as tall as its lines, the grid 4 px under it and down to 8 px
over Add."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    nh = h.note_h("label1", R - 8)
    h.set("label1", 8, 8, R - 8, nh)
    s.span(h, "gridMain", x=8, r=R, y=8 + nh + 4, b=h.n("butAdd").ny - 8)
