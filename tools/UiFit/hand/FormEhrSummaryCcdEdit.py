"""FormEhrSummaryCcdEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 884
x 639 by 14 x 15 to put its padding around the browser; the button row was main's scatter (the reconcile buttons from x 104, Show xml at
453, Print at 628, 181 px short of the right padding): main's size, the browser inside the padding down to 8 px over the button row; the
Reconcile caption (shown by the code with the three reconcile buttons) on the padding with the buttons 6 px after it, 8 px apart; Show
xml and Print on the right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_W, MAIN_H = 884, 639


def layout(h):
    s.size(h, MAIN_W, MAIN_H)
    yb = s.bar(h, right=("butShowXml", "butPrint"))
    h.set("webViewMain", 8, 8, MAIN_W - 16, yb - 8 - 8)
    bh = h.n("butPrint").nh
    lw = h.lw("labelReconcile")
    h.set("labelReconcile", 8, yb, lw, bh)
    x = 8 + lw + 6
    for name in ("butReconcileMedications", "butReconcileProblems", "butReconcileAllergies"):
        h.set(name, x, yb)
        x += h.n(name).nw + 8
