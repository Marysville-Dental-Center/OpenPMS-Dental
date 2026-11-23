"""FormPayPlanDynamic (Dynamic Payment Plan) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). One touch on
the planner's layout: Change Guarantor on the Guarantor row, 6 px after Go To at its top and height (the planner left it 2 px lower)."""


def layout(h):
    g = h.n("butGoToGuar")
    h.set("butChangeGuar", g.x + g.w + 6, g.y, None, g.h)
    # Fix round 8: the Terms group's content 8 px inside it on both sides: Handle Treatment Planned to its content edge (it ended 13 px
    # short); Create Schedule and Templates end on the Cancel/Save row's right edge (Save 6 px after Cancel), one right edge for the three rows.
    gt, tp = h.n("groupTerms"), h.n("groupTreatmentPlanned")
    h.set("groupTreatmentPlanned", None, None, gt.nw - 8 - tp.nx, None)
    sv = h.n("butSaveTerms")
    for name in ("butCreateSched", "butTemplates"):
        b = h.n(name)
        h.set(name, None, None, sv.nx + sv.nw - b.nx, None)
    # Fix round 9: the Patient and Guarantor rows end on the Terms group's right edge (the left column's): Go To and Change 6 px apart,
    # Change on the edge (it ended 67 px past it, 6 px from the tab control; main: on the column), the name boxes narrower by as much,
    # Category ending with them; the tab control, the Note box and its caption 8 px after the column (the window narrower by as much).
    R = gt.nx + gt.nw
    cg, gg = h.n("butChangeGuar"), h.n("butGoToGuar")
    xc = R - cg.nw
    xg = xc - 6 - gg.nw
    wt = xg - 6 - h.n("textGuarantor").nx
    h.set("butChangeGuar", xc, None)
    for b, t in (("butGoToGuar", "textGuarantor"), ("butGoToPat", "textPatient")):
        h.set(b, xg, None)
        h.set(t, None, None, wt, None)
    h.set("comboCategory", None, None, h.n("textPatient").nx + wt - h.n("comboCategory").nx, None)
    tc = h.n("tabControl1")
    dx = R + 8 - tc.nx
    for name in ("tabControl1", "textNote", "label10"):
        h.set(name, h.n(name).nx + dx, None)
    s = h.n("butSave")
    h.set("butSave", s.nx + dx, None)
    h.set(".", None, None, h.tree.nw + dx, h.tree.nh)
