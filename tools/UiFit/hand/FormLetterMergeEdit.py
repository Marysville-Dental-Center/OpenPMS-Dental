"""FormLetterMergeEdit on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree). The selection hint (a borderless
text box) is two lines at its width; main and the planner gave it one line (23 px), so its second line was cut: it is 34 px tall, the
Referred From and Other captions and lists under it move down by as much, and the three lists end on one bottom, 8 px over the bottom bar."""


def layout(h):
    root = h.tree
    yb = root.h - 8 - 24
    hint = h.n("textBox1")
    h.set("textBox1", None, None, None, 34)
    yc = hint.ny + 34 + 4
    for cap, lst in (("labelReferredFrom", "listReferral"), ("label6", "listOther")):
        h.set(cap, None, yc, None, 17)
        h.set(lst, None, yc + 17 + 2, None, yb - 8 - (yc + 17 + 2))
    p = h.n("listPatSelect")
    h.set("listPatSelect", None, None, None, yb - 8 - p.ny)
