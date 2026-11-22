"""FormPayPlan (Old Payment Plan, Insurance Payment Plan) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree).
Three touches on the planner's layout: the Current Totals boxes are the amortization grid's column totals and stand under its columns, as on
main (the planner spread them 6 px apart past the grid's right edge and widened the window to 1285 px): each box keeps main's offset from the
grid and its width, the caption right-aligned before them and Exclude Past Activity at the grid's left edge; the note "This should usually
match the total amount of the pay plan." under the Total Tx Amt row (the planner moved Total Cost onto it) with the hidden signature under
it; the Note box under the grid on its left and right edges. The window is 1125 px wide (the grid's right edge + 8). FormPayPlan.cs moves
the note up by the row pitch when the Total Tx Amt row is hidden, and lines Date of Agreement up with the Terms fields by their offset (0 on
the grid), where main moved them by fixed amounts (20 and 22 px) that fit main's layout."""

MAIN_GRID_X = 380
MAIN_TOTALS = (("textPrincipal", 625, 60), ("textInterest", 685, 52), ("textDue", 737, 60), ("textPayment", 797, 60),
               ("textAdjustment", 857, 71), ("textBalance", 928, 73))


def layout(h):
    H = h.H
    grid = h.n("gridCharges")
    y = grid.y + grid.h + 4
    for name, x, w in MAIN_TOTALS:
        h.set(name, grid.x + x - MAIN_GRID_X, y, w, H)
    lt = h.lw("labelTotals")
    x0 = grid.x + MAIN_TOTALS[0][1] - MAIN_GRID_X
    h.set("labelTotals", x0 - 6 - lt, y, lt, H)
    h.text_align("labelTotals", "MiddleRight")
    h.check_left("checkExcludePast")
    h.set("checkExcludePast", grid.x, y + (H - h.CH) // 2, h.cw("checkExcludePast"), h.CH)
    # Total Cost of Loan on the left column's field column (its label is the column's widest; the planner put the box 36 px left).
    ref = h.n("textAccumulatedDue")
    l15 = h.lw("label15")
    h.set("textTotalCost", ref.x, None, ref.w)
    h.set("label15", ref.x - 6 - l15, None, l15)
    h.text_align("label15", "MiddleRight")
    # The note under the last row of the left column, the hidden signature under it.
    tx = h.n("textTotalTxAmt")
    yn = tx.y + tx.h + 4
    h.set("labelTxAmtInfo", 8, yn, h.lw("labelTxAmtInfo"), 18)
    h.set("labelSignature", 8, yn + 18 + 4, None, 18)
    h.set("signatureBoxWrapper", 8, yn + 18 + 4 + 18 + 2)
    # The Note box under the grid on its edges.
    note = h.n("textNote")
    h.set("label10", grid.x, None, grid.w)
    h.set("textNote", grid.x, None, grid.w)
    WW = grid.x + grid.w + 8
    h.set("butSave", WW - 8 - 75, None)
    h.set(".", None, None, WW, h.tree.h)
