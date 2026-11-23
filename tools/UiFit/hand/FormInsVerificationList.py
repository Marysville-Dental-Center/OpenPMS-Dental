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
    # Fix round 8: the Verify page's bottom groups. Insurance Plan's Group Number row back on the row pitch (it stood under the plan note box
    # to its right, 9 px lower), the group to its content; Subscriber 8 px after it (5 px), its Pat Birthdate row under it on the row pitch
    # ending on Insurance Plan's bottom (main: so), the hidden integrity warning after it.
    gi, gs = h.n("groupInsurancePlan"), h.n("groupSubscriber")
    yg = h.n("textInsPlanGroupName").ny + h.H + 4
    for name in ("labelGroupNumber", "textInsPlanGroupNumber"):
        h.set(name, None, yg)
    h.set("groupInsurancePlan", None, None, None, yg + h.H + 8)
    dx = gi.nx + gi.nw + 8 - gs.nx
    for name in ("groupSubscriber", "labelPatBirthdate", "textPatBirthdate", "warningIntegrity1"):
        h.set(name, h.n(name).nx + dx, None)
    yp = gi.ny + gi.nh - h.H
    for name in ("labelPatBirthdate", "textPatBirthdate"):
        h.set(name, None, yp)
