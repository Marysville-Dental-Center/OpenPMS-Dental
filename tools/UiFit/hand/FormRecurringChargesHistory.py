"""FormRecurringChargesHistory on the grid (UI-12 slice 4, layout script on the planner's tree). The clinic picker's box stood 6 px left
of the Status box over it and ended 12 px short of it (GridRulesTests' picker column): the picker's box on the Status box's left and right
edges."""


def layout(h):
    s = h.n("comboStatuses")
    x = s.nx - h.clinic_w("comboClinics")
    h.set("comboClinics", x, None, s.nx + s.nw - x, None)
