"""The three reconcile windows' common layout (FormReconcileAllergy, FormReconcileMedication, FormReconcileProblem; UI-12 slice 4): the
two source grids one width, 8 px apart, from the content edge to the right content edge; each Add (an icon button, 60 px: 6 px between the
icon and its text and after the text) right-aligned under its grid; the reconciled grid 4 px under the Add row across the content, down to
4 px over Remove Selected (centred under it); the Save note right-aligned 6 px before Save on the bottom row (main's arrangement); the
window keeps its height (not smaller than main's 676)."""


def layout(h, existing, imported, reconcile, client=None):
    root = h.tree
    W = client[0] if client else root.w
    Hh = client[1] if client else root.h
    if client:
        h.set(".", None, None, W, Hh)
    R = W - 8
    w = (R - 8 - 8) // 2
    g = h.n(existing)
    top = 8
    gh = g.nh
    h.set(existing, 8, top, w, gh)
    h.set(imported, 8 + w + 8, top, R - 8 - w - 8, gh)
    ya = top + gh + 4
    h.set("butAddExist", 8 + w - 60, ya, 60, 24)
    h.set("butAddNew", R - 60, ya, 60, 24)
    yb = Hh - 8 - 24
    yr = yb - 4 - 24
    yg = ya + 24 + 4
    h.set(reconcile, 8, yg, R - 8, yr - 4 - yg)
    h.set("butRemoveRec", (W - 99) // 2, yr, 99, 24)
    h.set("butSave", R - 75, yb, 75, 24)
    lab = h.n("labelBatch")
    h.set("labelBatch", R - 75 - 6 - lab.nw, yb, lab.nw, 24)
