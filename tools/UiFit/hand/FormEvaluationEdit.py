"""FormEvaluationEdit on the grid (UI-12 slice 5, layout script on the planner's tree). The two grids stood at 18 and 6 px apart: from the
content edge, 8 px apart, the Grading Scale grid (its width kept) ending on the right content edge."""


def layout(h):
    R = h.tree.nw - 8
    gg = h.n("gridGrades")
    gx = R - gg.nw
    h.set("gridGrades", gx, None)
    h.set("gridCriterion", 8, None, gx - 8 - 8, None)
    # The two overall grade rows on the window's field column (263; they stood at 253 with their labels 1 px before the boxes): each
    # pair of boxes 1 px apart as drawn, the labels right-aligned 6 px before.
    fx = h.n("textDate").nx
    for lab, a, b in (("label8", "textGradeNumber", "textGradeNumberOverride"), ("label7", "textGradeShowing", "textGradeShowingOverride")):
        n = h.n(a)
        h.set(a, fx, None)
        h.set(b, fx + n.nw + 1, None)
        lw = h.lw(lab)
        h.set(lab, fx - 6 - lw, n.ny, lw, h.H)
        h.text_align(lab, "MiddleRight")
