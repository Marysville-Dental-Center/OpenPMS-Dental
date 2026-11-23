"""FormFeeScheds on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree). The planner kept the grid and the Check
Ins Plan Fee Schedules group at main's x 17 and the group's note 229 px wide, three lines in 42 px (cut): the grid and the group at the
content edge (8) to their right edge (374); in the group the note to 6 px before Go (two lines), Go on the content's right edge, both
centred on the group's content; the group as tall as its content + 8, ending where it ended; the grid down to 8 px over it."""


def layout(h):
    H = h.H
    R = 374
    grp = h.n("groupBox7")
    gb = grp.ny + grp.nh
    top = (grp.title_bottom or 18) + 4
    gw = R - 8
    lh = 32
    gh = top + lh + 8
    h.set("groupBox7", 8, gb - gh, gw, gh)
    b = h.n("butIns")
    h.set("butIns", gw - 8 - b.nw, top + (lh - H) // 2, b.nw, H)
    h.set("label6", 8, top, gw - 8 - b.nw - 6 - 8, lh)
    h.text_align("label6", "MiddleLeft")
    g = h.n("gridMain")
    h.set("gridMain", 8, g.ny, R - 8, gb - gh - 8 - g.ny)
