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
