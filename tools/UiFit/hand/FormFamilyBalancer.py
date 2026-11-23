"""FormFamilyBalancer on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The window's left content edge at 8
(the planner kept main's 12 for its anchored left column; the right edge is at 8). Fix round 8: the Changed Since Date check box holds three
lines (its text has two line breaks: the last, "NOT for normal users. NOT for conversions.", is its warning), so it keeps a three-line
height with its box at the top (the planner's rule makes a check box drawn taller than one line one line tall); the Changed Since Date row
and everything under it move down by as much, the group and the window grow. The As of Date note is as wide as its widest line (its three
lines fit its 47 px, at 310 px its last line was cut)."""


def layout(h):
    h.frame_left()
    gi = h.n("groupBoxIncomeTransfers")
    ck = h.n("checkChangedSinceDate")
    # Its three lines as the program's check box draws them (GDI+, Segoe UI 9: 408 x 50) and 2 px of the line's descent.
    CH3 = 50
    dy = CH3 - ck.nh
    y0 = ck.ny + ck.nh
    for k in gi.children:
        if k is not ck and k.ny >= y0:
            h.set(k.name, None, k.ny + dy)
    h.set("checkChangedSinceDate", None, None, None, CH3)
    if ck.check_align != "TopLeft":
        ck.new_check_align = "TopLeft"
    gb = gi.ny + gi.nh
    h.set("groupBoxIncomeTransfers", None, None, None, gi.nh + dy)
    for k in h.tree.children:
        if k is not gi and k.dock in ("None", "") and k.ny >= gb:
            h.set(k.name, None, k.ny + dy)
    h.set(".", None, None, h.tree.nw, h.tree.nh + dy)
    # The As of Date note: 317 px for its widest line (TextRenderer, Segoe UI 9), to the group's content edge.
    nd = h.n("labelAsOfDateDesc")
    h.set("labelAsOfDateDesc", None, None, gi.nw - 8 - nd.nx, None)
    # The Delete All Transfers check box on the column of the date boxes and the Changed Since check box (6 px left of it), its label
    # ending 6 px before it.
    cd, ld = h.n("checkDeleteTransfers"), h.n("labelDeleteTransfers")
    xc = ck.nx
    h.set("checkDeleteTransfers", xc, None, cd.nx + cd.nw - xc, None)
    h.set("labelDeleteTransfers", None, None, xc - 6 - ld.nx, None)
