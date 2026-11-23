"""FormProcSelect on the grid (UI-12 slice 5, layout script on the planner's tree). The filter group and the grid stood at 15; Negative
Adjs' label was 2 px wider than the others and its amount 2 px further right; the amounts were 61 px for a "$99,999.99" the code writes
(bold 69 px on Amt Original and Amt End: grid-runtime-texts.tsv, slice 4's FormAdjustSelect lesson): the group and the grid at the
content edge; the Breakdown's labels one column, its amounts one 71 px column, the group as wide as both (on the right content edge) with
its separator line, the grid to 8 px before it."""

AMOUNTS = ("labelAmtOriginal", "labelPositiveAdjs", "labelNegativeAdjs", "labelPayPlanProduction", "labelPaySplits", "labelInsPay",
           "labelWriteOff", "labelInsEst", "labelWriteOffEst", "labelCurrentSplits", "labelAmtEnd")


def layout(h):
    R = h.tree.nw - 8
    h.set("groupCreditLogic", 8, None)
    ref, val = h.n("label3"), h.n("labelPositiveAdjs")
    h.set("label7", ref.nx, None, ref.nw, None)
    for name in AMOUNTS:
        h.set(name, val.nx, None, 71, None)
    gw = val.nx + 71 + 8
    h.set("groupBreakdown", R - gw, None, gw, None)
    h.set("label8", None, None, gw - 16, None)
    h.set("gridMain", 8, None, R - gw - 8 - 8, None)
