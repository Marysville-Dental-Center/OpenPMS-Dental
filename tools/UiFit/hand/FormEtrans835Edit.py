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
    # Fix round 8: the Balancing group on the payer field column's right edge (its content ended 32 px short of its right side), its three
    # sums spread over its content with the - and = signs 6 px from them, each caption over its box (the planner had put the captions on a
    # row of their own, 6 px apart, off their boxes; main: over them).
    gb = h.n("groupBalancing")
    R = h.n("textPayerName").nx + h.n("textPayerName").nw
    h.set("groupBalancing", None, None, R - gb.nx, None)
    inner = (R - gb.nx) - 16
    ops = (("label18", 16), ("label20", 20))
    W = (inner - sum(w for _, w in ops) - 4 * 6) // 3
    x = 8
    boxes = (("textClaimInsPaidSum", "labelEquation1"), ("textProjAdjAmtSum", "labelEquation2"), ("textPayAmountCalc", "labelEquation3"))
    for i, (box, cap) in enumerate(boxes):
        w = W if i < 2 else 8 + inner - x
        h.set(box, x, None, w, None)
        h.set(cap, x, None, w, None)
        x += w + 6
        if i < 2:
            op, ow = ops[i]
            h.set(op, x, None, ow, None)
            x += ow + 6
