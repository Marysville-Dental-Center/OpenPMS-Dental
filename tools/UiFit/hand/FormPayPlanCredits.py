"""FormPayPlanCredits on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the window 937 x 510 -> 943 x 518;
Print and the grid stood 12 px from the left, the grid 12 px over the totals row, which stood 6 px over Save's row: main's size, on the
padding, the Credit Information group (with Clear and its note) on the right padding, the grid 8 px before it, the total on Save's row
ending on the grid's right edge, the grid 12 px over it; the two check boxes 6 px after Print."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 937, 510)
    R = s.right(h)
    gx = R - h.n("groupCreditInfo").nw
    dx = gx - h.n("groupCreditInfo").nx
    s.move(h, "groupCreditInfo", x=gx)
    s.move(h, "butClear", x=h.n("butClear").nx + dx)
    s.move(h, "label3", x=h.n("label3").nx + dx)
    y = s.bar(h, right_=("butSave",))
    r = gx - 8
    h.set("textTotal", r - h.n("textTotal").nw, y, None, None)
    s.label_before(h, "label7", "textTotal")
    h.set("butPrint", 8, 8, None, None)
    s.row_left(h, ("checkHideUnattached", "checkShowImplicit"), 8 + h.n("butPrint").nw + 6)
    s.fill(h, "gridMain", y=36, r=r, b=y - 12)
