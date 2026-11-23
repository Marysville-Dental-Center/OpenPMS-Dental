"""FormProcSelect on the grid (UI-12 slice 5, layout script on the planner's tree). The filter group and the grid stood at 15; Negative
Adjs' label was 2 px wider than the others and its amount 2 px further right (drawn 13 px right of the column): the group and the grid at
the content edge, the grid to 8 px before the Breakdown group; the label and amount on the Breakdown's two columns."""


def layout(h):
    h.set("groupCreditLogic", 8, None)
    b = h.n("groupBreakdown")
    h.set("gridMain", 8, None, b.nx - 8 - 8, None)
    ref, val = h.n("label3"), h.n("labelPositiveAdjs")
    h.set("label7", ref.nx, None, ref.nw, None)
    h.set("labelNegativeAdjs", val.nx, None, val.nw, None)
