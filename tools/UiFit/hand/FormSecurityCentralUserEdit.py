"""FormSecurityCentralUserEdit on the grid (UI-12 slice 10, layout script on the planner's tree). The grid stood 12 px from the left, Edit 17
from the right, the grid 13 over the bottom: Edit on the right padding, the grid from the padding to 8 px before it and to the bottom
padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    s.move(h, "butEdit", x=R - h.n("butEdit").nw, y=8)
    s.fill(h, "gridCentralUsers", y=8, r=h.n("butEdit").nx - 8)
