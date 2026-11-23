"""FormEtrans835Edit (ERA 835) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The planner put the right
column's first two fields (Payee Name, Payee Medicaid ID/NPI/TIN) 6 px left of the column's other fields (their label is the column's widest):
the script puts them on the column's x (the other five fields'), with the same right edges, the labels right-aligned 6 px before."""


def layout(h):
    ref = h.n("textTransHandlingDesc")
    x = ref.x
    for lab, fld in (("label15", "textPayeeName"), ("labelPayeeIdType", "textPayeeID")):
        f = h.n(fld)
        right = f.x + f.w
        w = right - x if f.w > 120 else h.n("textAcctNumEndingIn").w
        h.set(fld, x, None, w)
    for lab in ("label15", "labelPayeeIdType"):
        if lab in h.by_name:
            l = h.n(lab)
            lw = max(h.lw(lab), x - 6 - l.x)
            h.set(lab, x - 6 - lw, None, lw)
            h.text_align(lab, "MiddleRight")
    # Fix round 8: the Balancing group's content 8 px inside it on both sides (its content ended 32 px short of its right side): the middle
    # sum on the payer field column (the window's column check pairs it with the payer fields through the - sign before it), the three sums
    # one width with the - and = signs 6 px from them, each caption over its box (the planner had put the captions on a row of their own, 6 px
    # apart, off their boxes; main: over them), the group to its content.
    gb = h.n("groupBalancing")
    col = h.n("textPayerName").nx - gb.nx
    W = col - 8 - 6 - 16 - 6
    x = 8
    boxes = (("textClaimInsPaidSum", "labelEquation1", ("label18", 16)), ("textProjAdjAmtSum", "labelEquation2", ("label20", 20)),
             ("textPayAmountCalc", "labelEquation3", None))
    for box, cap, op in boxes:
        h.set(box, x, None, W, None)
        h.set(cap, x, None, W, None)
        x += W + 6
        if op:
            h.set(op[0], x, None, op[1], None)
            x += op[1] + 6
    h.set("groupBalancing", None, None, x - 6 + 8, None)
