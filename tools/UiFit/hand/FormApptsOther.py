"""FormApptsOther on the grid (UI-12 slice 4, layout script on the planner's tree). The bottom row started at x 31 (the grid at 12), its
buttons 9 px apart, and the grid and the two note boxes ended 7 px short of OK's edge: the bottom row from the grid's x, 6 px apart; Entire
Family (it schedules the family's recalls) over Schedule Recall; the grid and the note boxes to the right content edge (OK's)."""


def layout(h):
    R = h.tree.w - 8
    x = h.n("gridMain").nx
    for name in ("butGoTo", "butPin", "butRecall", "butNote", "butNew"):
        n = h.n(name)
        h.set(name, x, None)
        if name == "butRecall":
            h.set("butRecallFamily", x, None)
        x += n.nw + 6
    g = h.n("gridMain")
    h.set("gridMain", None, None, R - g.nx, None)
    for name in ("textApptModNote", "textFinUrg"):
        n = h.n(name)
        h.set(name, None, None, R - n.nx, None)
