"""FormEvaluationEdit on the grid (UI-12 slice 5, layout script on the planner's tree). The two grids stood at 18 and 6 px apart: from the
content edge, 8 px apart, the Grading Scale grid (its width kept) ending on the right content edge."""


def layout(h):
    R = h.tree.nw - 8
    gg = h.n("gridGrades")
    gx = R - gg.nw
    h.set("gridGrades", gx, None)
    h.set("gridCriterion", 8, None, gx - 8 - 8, None)
