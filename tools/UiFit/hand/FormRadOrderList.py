"""FormRadOrderList on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the window 974 -> 982 wide and kept the
note and the grid 12 px from the left, the grid 13 over the bottom: main's size, on the padding, the Approve group on the right padding and
the grid 8 px before it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 974, 696)
    R = s.right(h)
    s.move(h, "groupBox1", x=R - h.n("groupBox1").nw)
    h.set("label1", 8, None, R - 8, None)
    s.fill(h, "gridMain", r=h.n("groupBox1").nx - 8)
