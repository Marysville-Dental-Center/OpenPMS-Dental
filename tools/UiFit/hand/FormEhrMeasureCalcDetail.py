"""FormEhrMeasureCalcDetail on the grid (UI-12 slice 5, layout script on the planner's tree). The review note (its text set by the code),
the double-click hint and the patients grid stood at 12 (Cancel at 8) and the fields ended 3 px short of Close's edge: the notes and the
grid at the content edge, every field, the notes and the grid to the right content edge."""


def layout(h):
    R = h.tree.nw - 8
    for name in ("labelInternalReview", "labelHint", "gridAudit"):
        h.set(name, 8, None, R - 8, None)
    for name in ("textMeasureName", "textPeriod", "textObjective", "textMeasure", "textNumeratorExplain", "textDenominatorExplain",
                 "textExclusionExplain", "textExclusionCount", "textResult"):
        n = h.n(name)
        h.set(name, None, None, R - n.nx, None)
