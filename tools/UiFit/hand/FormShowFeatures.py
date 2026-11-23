"""FormShowFeatures on the grid (UI-12 slice 5, layout script on the planner's tree). The planner put the right column's groups at two x
(Insurance at 261, Special Use Only at 269, 16 px after the Enterprise group beside it): both 8 px after the left column, to the right
content edge."""


def layout(h):
    R = h.tree.nw - 8
    x = h.n("groupChargeTools").nx + h.n("groupChargeTools").nw + 8
    for name in ("groupInsurance", "groupSpecialUseOnly"):
        h.set(name, x, None, R - x, None)
