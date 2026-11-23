"""FormDiscountPlans on the grid (UI-12 slice 7, layout script on the planner's tree). Fix round (the lead's review of 2026-11-23): the planner
left the side column 22 px before OK's right edge and the grid at 13: Add and Merge at OK's x, Show Hidden (20 px wider than the buttons)
ending on OK's edge, the grid from the content edge to 8 px before the check box."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    ok = h.n("butOK")
    E = ok.nx + ok.nw
    for n in ("butAdd", "butMerge"):
        s.move(h, n, x=E - h.n(n).nw)
    c = h.n("checkShowHidden")
    s.move(h, "checkShowHidden", x=E - c.nw)
    s.span(h, "gridMain", x=8, r=c.nx - 8)
