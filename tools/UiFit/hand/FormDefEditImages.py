"""FormDefEditImages on the grid (UI-12 slice 4, layout script on the planner's tree). The Name caption was centred over a box at x 23
and the Usage group stood 16 px lower than it, ending 26 px short of Save's edge: the caption (left-aligned, 2 px over its box), the box
and Hidden at the content edge; the Usage group at the content top, to the right content edge."""


def layout(h):
    R = h.tree.w - 8
    h.set("labelName", 8, 8, h.n("textName").nw, 18)
    h.text_align("labelName", "BottomLeft")
    h.set("textName", 8, 8 + 18 + 2)
    h.check_left("checkHidden")
    h.set("checkHidden", 8, 8 + 18 + 2 + h.H + 4)
    g = h.n("groupBox1")
    h.set("groupBox1", None, 8, R - g.nx, None)
