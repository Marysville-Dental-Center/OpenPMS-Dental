"""FormEvaluationEdit on the grid (UI-12 slice 5, layout script on the planner's tree). The two grids stood at 18 and 6 px apart: from the
content edge, 8 px apart, the Grading Scale grid (its width kept) ending on the right content edge. The overall grade rows: see layout()."""


def layout(h):
    R = h.tree.nw - 8
    gg = h.n("gridGrades")
    gx = R - gg.nw
    h.set("gridGrades", gx, None)
    h.set("gridCriterion", 8, None, gx - 8 - 8, None)
    # The two overall grade rows on the window's field column (263; they stood at 253 with their labels 1 px before the boxes), the
    # labels right-aligned 6 px before. Fix round 2: the four boxes were 20 px tall on a 26 px pitch (3 px apart) with each override box
    # 1 px after its computed box: the field height on the 27 px pitch, the lower row on the Delete / Save row, each override box 6 px
    # after its computed box.
    fx = h.n("textDate").nx
    y2 = h.n("butSave").ny
    rows = (("label8", "textGradeNumber", "textGradeNumberOverride", y2 - 27),
            ("label7", "textGradeShowing", "textGradeShowingOverride", y2))
    for lab, a, b, y in rows:
        n = h.n(a)
        h.set(a, fx, y, None, h.H)
        h.set(b, fx + n.nw + 6, y, None, h.H)
        lw = h.lw(lab)
        h.set(lab, fx - 6 - lw, y, lw, h.H)
        h.text_align(lab, "MiddleRight")
