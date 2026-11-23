"""FormInsFilingCodes on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 17 px from the left and 28 px before
the button column, Up and Down 5 px narrower than Add and None: the grid from the content edge to 8 px before the buttons, the four
buttons 80 px."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "gridMain", x=8, r=h.n("butAdd").nx - 8)
    for n in ("butUp", "butDown"):
        h.set(n, None, None, 80, None)
