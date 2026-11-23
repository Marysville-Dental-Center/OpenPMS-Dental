"""FormShowFeatures on the grid (UI-12 slice 5, layout script on the planner's tree). The planner put the right column's groups at two x
(Insurance at 261, Special Use Only at 269, 16 px after the Enterprise group beside it): both 8 px after the left column, to the right
content edge.

Fix round 2: in Charge Tools the framed radio pair (Billing/Finance Charges, Late Charges) stood at 47 (190 px wide), 47 px inside the
group on the left against 8 on the right, with Repeating Charges and every other check box of the window on the content edge: its frame
on the content edge too (8, to the right content edge), its height kept."""


def layout(h):
    R = h.tree.nw - 8
    x = h.n("groupChargeTools").nx + h.n("groupChargeTools").nw + 8
    for name in ("groupInsurance", "groupSpecialUseOnly"):
        h.set(name, x, None, R - x, None)
    ct = h.n("groupChargeTools")
    h.set("groupBoxCharges", 8, None, ct.nw - 8 - 8, None)
