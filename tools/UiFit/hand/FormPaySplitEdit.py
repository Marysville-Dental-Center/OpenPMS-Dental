"""FormPaySplitEdit (Paysplit) on the grid (UI-12 slice 1 fix round 9, layout script of grid_hand.py, on the planner's tree). Two touches on
the planner's layout:
- the Provider row (box, pick button, label) on the field pitch right after Clinic (the planner kept it 16 px lower, level with the hidden
  Edit Anyway row on the right, which keeps its row);
- one right edge for the right-hand blocks: the Procedure/Adjustment tab control's (the Patient group ended 13 px short of it, Save 25 px
  past it): the Patient group, its patient box and list, the Edit Anyway row and Save end on it, the window narrower by as much."""


def layout(h):
    cl, pr, pk, l5 = h.n("comboClinic"), h.n("comboProvider"), h.n("butPickProv"), h.n("label5")
    y = cl.ny + h.H + 4
    for name in ("comboProvider", "butPickProv", "label5"):
        h.set(name, None, y)
    tc, gp, sv = h.n("tabControl"), h.n("groupPatient"), h.n("butSave")
    R = tc.nx + tc.nw
    h.set("groupPatient", None, None, R - gp.nx, None)
    for name in ("textPatient", "listPatient"):
        n = h.n(name)
        h.set(name, None, None, (R - gp.nx) - 8 - n.nx, None)
    ea, le = h.n("butEditAnyway"), h.n("labelEditAnyway")
    dx = R - (ea.nx + ea.nw)
    h.set("butEditAnyway", ea.nx + dx, None)
    h.set("labelEditAnyway", le.nx + dx, None)
    dw = R + 8 - h.tree.nw
    h.set("butSave", sv.nx + dw, None)
    h.set(".", None, None, h.tree.nw + dw, h.tree.nh)
