"""ControlApptPanel (the appointment book) on the grid (UI-12 slice 16): the book is drawn by its code, which also places and sizes its two
scroll bars from the drawn columns at run time; in the designer the horizontal bar now ends 1 px before the vertical one (it ran 43 px
under it), as the code leaves it."""

KEEP_BAR_GAP = True


def layout(h):
    v = h.n("vScrollBar1")
    hb = h.n("hScrollBar1")
    h.set("hScrollBar1", hb.nx, hb.ny, v.nx - 1 - hb.nx, hb.nh)
