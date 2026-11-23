"""FormRpInsPayPlansPastDue on the grid (UI-12 slice 4, layout script on the planner's tree). The filter row stood at y 13 with the Provs
label at 8 (its text 5 px over the combo's) and Days past due 34 px tall: one row at the content top, each label right-aligned 6 px before
its field and centred on it, the grid 8 px under the row (its bottom kept)."""


def layout(h):
    y = 8
    H = h.H
    for lab, fld in (("label2", "textDaysPastDue"), ("labelProv", "comboProvs")):
        f = h.n(fld)
        lw = h.lw(lab)
        h.set(fld, None, y, None, H)
        h.set(lab, f.nx - 6 - lw, y, lw, H)
        h.text_align(lab, "MiddleRight")
    h.set("comboClinics", None, y, None, H)
    g = h.n("gridMain")
    top = y + H + 8
    h.set("gridMain", None, top, None, g.ny + g.nh - top)
