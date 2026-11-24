"""FormPayPlanSelect on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the window 609 x 246 -> 610 x 258 and
kept the grid 12 px from the left, Show Active Only 21 px short of the right: main's size, on the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 609, 246)
    R = s.right(h)
    s.bar(h, left=("butNone",), right_=("butOK",))
    s.move(h, "checkShowActiveOnly", x=R - h.n("checkShowActiveOnly").nw)
    h.set("labelExpl", 8, None, None, None)
    s.fill(h, "gridMain", y=32, over="butOK")
