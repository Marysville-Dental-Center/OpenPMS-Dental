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
