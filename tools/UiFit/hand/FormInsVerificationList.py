"""FormInsVerificationList (Insurance Verification List) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree).
One touch on the planner's layout: the Current / Past Due tab control (and the grid on each of its pages) reaches the Verification List
page's content edge, the right edge of Verify Status and Status Note under it (it ended 24 px short)."""


def layout(h):
    page = h.n("tabVerify")
    t = h.n("tabControlVerificationList")
    w = page.w - 8 - t.x
    dw = w - t.w
    h.set("tabControlVerificationList", None, None, w, None)
    for p, g in (("tabCurrent", "gridMain"), ("tabPastDue", "gridPastDue")):
        h.set(p, None, None, h.n(p).w + dw, None)
        h.set(g, None, None, h.n(g).w + dw, None)
