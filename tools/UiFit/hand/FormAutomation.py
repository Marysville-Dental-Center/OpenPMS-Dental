"""FormAutomation on the grid (UI-12 slice 7, layout script on the planner's tree). The note and the grid stood 21 px from the left and 32 px
from the right, the grid 42 px over Add: both on the content edges, the grid down to 8 px over Add."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    s.move(h, "label1", x=8)
    s.span(h, "gridMain", x=8, r=R, b=h.n("butAdd").ny - 8)
