"""FormHL7DefMessageEdit on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The planner gave the hidden delete note
(shown beside the disabled Delete when the definition is internal) a row of its own, 56 px of empty band under the grid: the button row
8 px under the grid, Delete on the grid's x, the note between Delete and Add (6 px from each), centred on the row; the window 8 px under the
row.
UI-12F (the final consistency pass): main's 534 px high again (a window never shrinks) and the button row a section gap (12 px)
under the grid (the bottom bar's gap): the row on the bottom padding, the grid down to 12 px over it."""


def layout(h):
    g = h.n("gridMain")
    y = 534 - 8 - 24  # UI-12F: main's height, the row on the bottom padding
    h.set("gridMain", None, None, None, y - 12 - g.ny)
    d = h.n("butDelete")
    h.set("butDelete", g.nx, y, None, None)
    add = h.n("butAdd")
    h.set("butAdd", None, y, None, None)
    h.set("butSave", None, y, None, None)
    x = g.nx + d.nw + 6
    lab = h.n("labelDelete")
    h.set("labelDelete", x, y + (24 - lab.nh) // 2, add.nx - 6 - x, None)
    h.set(".", None, None, None, y + 24 + 8)
