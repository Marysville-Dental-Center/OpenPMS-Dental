"""FormRpInsPendingSupplementals on the grid (UI-12 slice 4, layout script on the planner's tree). The clinic picker's box stood 35 px
left of the Type box over it (GridRulesTests' picker column): the picker's box on the Type box's left and right edges."""


def layout(h):
    s = h.n("comboType")
    x = s.nx - h.clinic_w("comboClinics")
    h.set("comboClinics", x, None, s.nx + s.nw - x, None)
