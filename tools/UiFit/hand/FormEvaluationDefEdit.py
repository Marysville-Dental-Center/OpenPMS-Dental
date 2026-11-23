"""FormEvaluationDefEdit on the grid (UI-12 slice 7, layout script on the planner's tree). Total Points stood 2 px before its box and the grid
12 px from the left against Delete's 8: the label 6 px before its box on its row, the grid on the content edge. Fix round (the lead's review
of 2026-11-23): Add, Up and Down on Save's column (they stood 23 px left of it), the grid to 8 px before it; the Total Points row at the
field height with 12 px over the bottom bar and 12 px under the grid (it stood 4 px over the bar), its box ending on the grid's right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    sv = h.n("butSave")
    for n in ("butCriterionAdd", "butUp", "butDown"):
        s.move(h, n, x=sv.nx)
    R = sv.nx - 8
    y = sv.ny - 12 - h.H
    t = h.n("textTotalPoints")
    h.set("textTotalPoints", R - t.nw, y, None, h.H)
    s.label_before(h, "labelTotalPoint", "textTotalPoints")
    s.span(h, "gridMain", x=8, r=R, b=y - 12)
