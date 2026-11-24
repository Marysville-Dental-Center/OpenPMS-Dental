"""ServiceManager's FormMain on the grid (UI-12 slice 14, layout script; a plain exe's window). Main: the note, the list and Add 23 to 26 px
from the left edge, the note's last line cut by the list (98 px for its six lines at the window's width). The planner kept the 23 px
margin. Now: the note on the padding over the window's width, as tall as its lines; the list a row gap under it, Add on the bottom
padding with the list ending a section gap over it."""

PAD = 8


def layout(h):
    W, HH = h.tree.nw, h.tree.nh
    cw = W - 2 * PAD
    nh = h.note_h("label1", cw, estimate=True)
    h.set("label1", PAD, PAD, cw, nh)
    ya = HH - PAD - 24
    h.set("butAdd", PAD, ya, 75, 24)
    yl = PAD + nh + 4
    h.set("listMain", PAD, yl, 174, ya - 12 - yl)
