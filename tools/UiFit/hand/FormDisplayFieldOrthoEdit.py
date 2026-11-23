"""FormDisplayFieldOrthoEdit on the grid (UI-12 slice 3, layout script of grid_hand.py on the planner's tree). The planner left the three
notes after the boxes 17 px tall at the rows' tops ((optional) 3 px over its row's centre) and the order buttons between the first two rows:
each note on its box's row (23 px, MiddleLeft) 6 px after the box; Up and Down on the Description row, 6 px after its box, 6 px apart."""


def layout(h):
    for box, note in (("textDescriptionOverride", "label6"), ("textDescription", None)):
        if note:
            b = h.n(box)
            h.set(note, b.nx + b.nw + 6, b.ny, h.lw(note), h.H)
            h.text_align(note, "MiddleLeft")
    for lab, note in (("label4", "label5"), ("label3", "labelDefaultMode")):
        l = h.n(lab)
        n = h.n(note)
        h.set(note, n.nx, l.ny, h.lw(note), h.H)
        h.text_align(note, "MiddleLeft")
    d = h.n("textDescription")
    h.set("butUp", d.nx + d.nw + 6, d.ny, 25, h.H)
    h.set("butDown", d.nx + d.nw + 6 + 31, d.ny, 25, h.H)
