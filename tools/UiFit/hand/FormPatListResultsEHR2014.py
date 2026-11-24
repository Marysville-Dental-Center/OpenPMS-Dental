"""FormPatListResultsEHR2014 on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the window 886 x 370 -> 886 x
383 and kept Print 8 px short of the right: main's size, Print (anchored to the bottom, at 279 on main, moved to the top by the planner) on
the bottom and right padding, the grid 8 px before it and to the bottom padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 886, 370)
    R = s.right(h)
    s.move(h, "butPrint", x=R - h.n("butPrint").nw, y=s.bottom(h) - h.n("butPrint").nh)
    s.fill(h, "gridMain", y=8, r=h.n("butPrint").nx - 8)
