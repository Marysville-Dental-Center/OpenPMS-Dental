"""FormChartView on the grid (UI-12 slice 5, layout script on the planner's tree). Family and Super Family are Commlog's sub-items (main
drew them 18 px in under it); the planner put them on the list's edge: 16 px in (the check box lead) under Commlog."""


def layout(h):
    x = h.n("checkComm").nx + 16
    for name in ("checkCommFamily", "checkCommSuperFamily"):
        h.set(name, x, None)
