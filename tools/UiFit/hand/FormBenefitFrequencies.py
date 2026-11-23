"""FormBenefitFrequencies on the grid (UI-12 slice 7, layout script on the planner's tree). The grid filled 273 of the window's 393 px (108 px
of empty window beside it) and stopped 48 px over Save: from the content edge to Save's edge, 12 px over the bottom bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "gridMain", x=8, r=s.right(h))
    s.grid_bottom(h, "gridMain")
