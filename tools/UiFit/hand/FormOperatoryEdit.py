"""FormOperatoryEdit on the grid (UI-12 slice 3, layout script of grid_hand.py on the planner's tree). The planner kept the notes beside the
check boxes, Update All and Op Type at main's x and 1-2 px off their rows, and widened the window to 665 for a note 363 px wide: each note
6 px after the text or box it explains, on its row (centred, MiddleLeft), as wide as its text; the window as wide as its widest row + the
padding (618; main 589), Save at its right edge, 24 px tall like every bottom bar button, 8 px over the bottom edge. The clinic picker's
box ends on the field column's right edge (434, as Op Name, Provider, Hygienist and Op Type; the planner left it 4 px short)."""


def layout(h):
    root = h.tree
    H = h.H
    CH = h.CH
    right = 0
    for chk, note in (("checkIsHidden", "label8"), ("checkIsHygiene", "label9"), ("checkSetProspective", "label3")):
        c = h.n(chk)
        x = c.nx + h.cw(chk) + 6
        h.set(note, x, c.ny, h.lw(note), CH)
        h.text_align(note, "MiddleLeft")
        right = max(right, x + h.lw(note))
    for anchor, note in (("butUpdateProvs", "label11"), ("comboOpType", "label13")):
        a = h.n(anchor)
        x = a.nx + a.nw + 6
        h.set(note, x, a.ny, h.lw(note), H)
        h.text_align(note, "MiddleLeft")
        right = max(right, x + h.lw(note))
    for k in root.children:
        if k.visible and k.name not in ("butSave",):
            right = max(right, k.nx + k.nw)
    W = max(589, right + 8)
    s = h.n("butSave")
    h.set("butSave", W - 8 - s.nw, root.nh - 8 - 24, None, 24)
    cc = h.n("comboClinic")
    h.set("comboClinic", None, None, h.n("textOpName").nx + h.n("textOpName").nw - cc.nx, None)
    h.set(".", None, None, W, root.nh)
