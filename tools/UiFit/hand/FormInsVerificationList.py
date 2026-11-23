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
    for name in ("groupSubscriber", "warningIntegrity1"):
        h.set(name, h.n(name).nx + dx, None)
    # Fix round 9: the two groups one height (Subscriber as tall as Insurance Plan: they stand side by side on one top). The Pat Birthdate
    # row is the patient's, not the subscriber's, and is no child of the group: it stands in the right column, on the row under Status Note
    # (label right-aligned on the column's labels, its box on the column's fields).
    h.set("groupSubscriber", None, None, None, gi.nh)
    vs, sn, ls = h.n("comboSetVerifyStatus"), h.n("textInsVerifyReadOnlyNote"), h.n("labelVerifyStatusSet")
    lpb = h.lw("labelPatBirthdate")
    yp = sn.ny + sn.nh + 4
    h.set("labelPatBirthdate", ls.nx + ls.nw - lpb, yp, lpb, h.H)
    h.text_align("labelPatBirthdate", "MiddleRight")
    h.set("textPatBirthdate", vs.nx, yp, vs.nw, h.H)
    # Verification Filters: the Region list ends on Clear's right edge (main: so), Refresh on the Verify Status box's right edge.
    cl, lr = h.n("butCarrierClear"), h.n("listBoxVerifyRegions")
    h.set("listBoxVerifyRegions", None, None, cl.nx + cl.nw - lr.nx, None)
    fv, rf = h.n("comboFilterVerifyStatus"), h.n("butRefresh")
    h.set("butRefresh", fv.nx + fv.nw - rf.nw, None)
