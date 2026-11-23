"""FormEhrAmendments on the grid (UI-12 slice 7, layout script on the planner's tree). The note and the grid stood 15 px from the left, the
grid 17 px over the bottom: both on the content edge, the grid 8 px over the bottom (its right edge 6 px before Add kept)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "label3", x=8, r=s.right(h))
    s.span(h, "gridMain", x=8, b=s.bottom(h))
