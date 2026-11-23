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
    # Fix round 9: the Patient, Guarantor and Insurance Plan rows end on the left column's right edge (the groups', 8 + their width): Go To
    # and Change 6 px apart, Change on the edge (it ended 54 px past it, 4 px from the grid; main: on the column), the name boxes narrower by
    # as much, Category ending with them; Sheet on the edge; the grid and the Note box 8 px after the column (the window narrower by as much).
    gt = h.n("groupTerms")
    R = gt.nx + gt.nw
    cg, gg = h.n("butChangeGuar"), h.n("butGoToGuar")
    xc = R - cg.nw
    xg = xc - 6 - gg.nw
    wt = xg - 6 - h.n("textGuarantor").nx
    h.set("butChangeGuar", xc, None)
    for b, t in (("butGoToGuar", "textGuarantor"), ("butGoToPat", "textPatient")):
        h.set(b, xg, None)
        h.set(t, None, None, wt, None)
    h.set("comboCategory", None, None, h.n("textPatient").nx + wt - h.n("comboCategory").nx, None)
    cp, ti = h.n("butChangePlan"), h.n("textInsPlan")
    h.set("butChangePlan", R - cp.nw, None)
    h.set("textInsPlan", None, None, R - cp.nw - 6 - ti.nx, None)
    cs = h.n("comboSheetDef")
    ls = h.lw("labelSheetDef")
    h.set("comboSheetDef", R - cs.nw, None)
    h.set("labelSheetDef", R - cs.nw - 6 - ls, None, ls, None)
    h.text_align("labelSheetDef", "MiddleRight")
    h.set("gridCharges", R + 8, None)
    grid = type("G", (), {"x": grid.nx, "y": grid.ny, "w": grid.nw, "h": grid.nh})
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
