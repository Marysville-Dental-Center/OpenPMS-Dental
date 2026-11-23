"""FormOrthoChart on the grid (UI-12 slice 3, mode hand). Main's arrangement on the grid's numbers under the menu: the patient field grid
from the padding, the date range row (its label, box and picker button) under it ending on its right edge; the right column at the right
edge: the ortho grid (shown by the code), User with its box and Change User, the signature box, the provider note (shown by the code) on the
date range row; the tab strip and the ortho chart grid under them to the edges (the grid touching the strip, as main); the bottom bar: Add
Row, Auto Note and Delete at the left with the lock note and Take Control (shown by the code) after them, Audit Trail and Print before
Save at the right."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    top = h.n("menuMain").nh + PAD
    sw = h.n("signatureBoxWrapper").nw
    RC = R - sw
    gp = h.n("gridPat")
    gr = RC - GAP
    h.set("gridPat", PAD, top, gr - PAD, gp.nh)
    dy = top + gp.nh + 4
    x = gr - 24
    h.set("butShowDateRange", x, dy, 24, H)
    x -= 6 + 148
    h.set("textDateRange", x, dy, 148, H)
    lw = h.lw("label1")
    h.set("label1", x - 6 - lw, dy, lw, H)
    h.text_align("label1", "MiddleRight")
    go = h.n("gridOrtho")
    h.set("gridOrtho", RC, top, sw, go.nh)
    uy = top + go.nh + GAP
    ul = h.lw("labelUser")
    h.set("labelUser", RC, uy, ul, H)
    h.text_align("labelUser", "MiddleLeft")
    h.set("textUser", RC + ul + 6, uy, 116, H)
    h.set("butChangeUser", RC + ul + 6 + 116 + 6, uy, 91, H)
    sy = uy + H + 4
    h.set("signatureBoxWrapper", RC, sy, sw, h.n("signatureBoxWrapper").nh)
    h.set("labelPermAlert", RC, dy, sw, H)
    h.text_align("labelPermAlert", "MiddleLeft")
    ty = max(dy + H, sy + h.n("signatureBoxWrapper").nh) + GAP
    h.set("tabControl", PAD, ty, R - PAD, 21)
    by = HH - PAD - 24
    gy = ty + 20
    h.set("gridMain", PAD, gy, R - PAD, by - GAP - gy)
    x = PAD
    for b in ("butAdd", "butUseAutoNote", "butDelete"):
        h.set(b, x, by, 80, 24)
        x += 86
    x += 10
    ll = h.lw("labelLocked")
    h.set("labelLocked", x, by, ll, 24)
    h.text_align("labelLocked", "MiddleLeft")
    h.set("buttonTakeControl", x + ll + 6, by, 85, 24)
    h.set("butSave", R - 75, by, 75, 24)
    h.set("butPrint", R - 75 - 16 - 85, by, 85, 24)
    h.set("butAudit", R - 75 - 16 - 85 - 6 - 80, by, 80, 24)
