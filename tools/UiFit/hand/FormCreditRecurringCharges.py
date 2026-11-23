"""FormCreditRecurringCharges on the grid (UI-12 slice 5, layout script on the planner's tree). The grid stood at 12 and ended 55 px over
the bottom bar; the side column's groups ended at three x (1050, 1040, 967) with Refresh centred over them: the grid from the content edge
to 8 px before the side column and down to 8 px over the bar; Refresh, the Date Filter, Clinic Filter and Counts groups one width to the
right content edge (Today on the date group's content edge, the clinic list to its content edge)."""


def layout(h):
    R = h.tree.nw - 8
    x = h.n("groupDateFilter").nx
    w = R - x
    h.set("butRefresh", x, None, w, None)
    for name in ("groupDateFilter", "groupClinics", "groupCounts"):
        h.set(name, x, None, w, None)
    t = h.n("butToday")
    h.set("butToday", w - 8 - t.nw, None)
    d = h.n("textDate")
    h.set("textDate", None, None, w - 8 - t.nw - 6 - d.nx, None)
    h.set("listClinics", None, None, w - 16, None)
    for name in ("labelTotal", "labelSelected", "labelCharged", "labelUpdated", "labelFailed"):
        h.set(name, None, None, w - 16, None)
    bar = h.n("butAll").ny
    h.set("gridMain", 8, 8, x - 8 - 8, bar - 8 - 8)
