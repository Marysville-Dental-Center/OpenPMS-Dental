"""FormAutoNoteControls on the grid (UI-12 slice 7, layout script on the planner's tree). The grid ended 17 px from the right edge (8 from
the left) and 19 px over the bottom bar: to the right edge, 12 px over the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "gridMain", r=s.right(h))
    s.grid_bottom(h, "gridMain")
