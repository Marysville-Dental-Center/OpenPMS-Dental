"""FormInsFilingCodes on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 17 px from the left and 28 px before the
button column, Up and Down 5 px narrower than Add and None: the grid from the content edge to 8 px before the buttons, the four buttons 80
px. Fix round (the lead's review of 2026-11-23): the column on OK's edges: Add, Up, Down and None at OK's x and width (75 px; they stood 80
px wide 25 px left of it), the grid to 8 px before them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    ok = h.n("butOK")
    for n in ("butAdd", "butUp", "butDown", "butNone"):
        h.set(n, ok.nx, None, ok.nw, None)
    s.span(h, "gridMain", x=8, r=ok.nx - 8)
