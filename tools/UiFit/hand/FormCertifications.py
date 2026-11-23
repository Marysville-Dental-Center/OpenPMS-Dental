"""FormCertifications on the grid (UI-12 slice 3, mode hand: the planner kept the Cert Complete view's lists over the grid). Main's
arrangement on the grid's numbers: Setup at the padding; Employee Search and Reports To as captioned boxes in the first column with the View
by group beside them in the second; the Employee and Categories lists under their captions, one top and one height, down to the grid's
bottom. The Cert Complete view's lists and their captions (FormCertifications.cs moves them onto the Employee and Categories lists' places
and switches them by Visible) stand on those bounds already, and its Sort By Date Completed on the Category view's (one bounds, as the code
puts it). The grid from the third column to the right edge under the row of check boxes, Print under it at the bottom right."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    CH = h.CH
    by = HH - PAD - 24
    cw = h.n("listBoxEmployee").nw
    A = PAD
    B = A + cw + GAP
    G = B + cw + GAP
    h.set("butSetup", A, PAD, None, 24)
    y = PAD + 24 + GAP
    y0 = y
    y = h.captioned("labelEmpSearch", "textEmpSearch", A, y, cw) + GAP
    y = h.captioned("labelReportsTo", "comboSupervisor", A, y, cw)
    g = h.n("groupBoxOrderBy")
    top = (g.title_bottom or 18) + 4
    for i, r in enumerate(("radioCategory", "radioCertification")):
        h.check_left(r)
        h.set(r, PAD, top + i * (CH + 4), h.cw(r), CH)
    h.set("groupBoxOrderBy", B, y0, cw, y - y0)
    cy = y + GAP
    ly = cy + 17 + 2
    lb = by - GAP
    for cap, lst, x in (("labelEmployee", "listBoxEmployee", A), ("labelCategories", "listBoxCategories", B),
                        ("labelCategories2", "listBoxCategories2", A), ("labelCertification", "listBoxCertification", B)):
        h.set(cap, x, cy, cw, 17)
        h.text_align(cap, "BottomLeft")
        h.set(lst, x, ly, cw, lb - ly)
    yc = PAD + (24 - CH) // 2
    # Show Only Incomplete and the Cert Complete view's Sort By Date Completed on one bounds (the code puts one on the other's place).
    w1 = max(h.cw("checkIncomplete"), h.cw("checkSortDateCertComplete"))
    for c, x, w in (("checkIncomplete", G, w1), ("checkSortDate", G + w1 + 16, h.cw("checkSortDate")), ("checkSortDateCertComplete", G, w1)):
        h.check_left(c)
        h.set(c, x, yc, w, CH)
    h.set("gridMain", G, PAD + 24 + GAP, R - G, lb - (PAD + 24 + GAP))
    h.set("butPrint", R - h.n("butPrint").nw, by, None, 24)
    h.watch(".", "groupBoxOrderBy")
