"""FormFeeScheds on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree). The planner kept the grid and the Check
Ins Plan Fee Schedules group at main's x 17 and the group's note 229 px wide, three lines in 42 px (cut): the grid and the group at the
content edge (8) to their right edge (374); in the group the note to 6 px before Go (two lines), Go on the content's right edge, both
centred on the group's content; the group as tall as its content + 8, ending where it ended; the grid down to 8 px over it. The right
column (review 2026-11-23: the planner centred Sort, Add, Clean Up Allowed, Hide Unused and their notes on x 486, while Type, Show Hidden,
Up, Down and Set Order stand at 380, and main had all of them at one left edge): every button at 380, each note under its button at 380,
left-aligned, to the right content edge and as tall as its lines; the stack 4 px apart from Set Order down (the row gap of Up and Down)."""
import grid_tools as gt


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

    # The right column under Set Order: one left edge (the column of Up, Down and Set Order), 4 px between every item.
    X = h.n("butSetOrder").nx
    RC = h.tree.nw - 8
    y = max(h.n("butSetOrder").ny + h.n("butSetOrder").nh, h.n("labelSetOrder").ny + h.n("labelSetOrder").nh) + 4
    for but, note in (("butSort", "labelSort"), ("butAdd", None), ("butCleanUp", "labelCleanUp"), ("butHideUnused", "labelHideUnused")):
        bn = h.n(but)
        # A button at least its text + 6 px on each side (Clean Up Allowed's 101 px text filled its 102 px).
        bw = max(bn.nw, (bn.text_w or 0) + 14)
        h.set(but, X, y, bw, bn.nh)
        y += bn.nh + 4
        if note:
            ln = h.n(note)
            room = RC - X
            lines = max(gt.wrapped_lines(ln.text_w, room - 4), gt.note_lines(ln.text, ln.text_w, room - 4))
            nh = lines * (ln.text_h or 16) + 2
            h.set(note, X, y, room, nh)
            h.text_align(note, "TopLeft")
            y += nh + 4
