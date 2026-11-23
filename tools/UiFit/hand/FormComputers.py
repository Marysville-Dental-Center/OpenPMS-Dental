"""FormComputers on the grid (UI-12 slice 5, layout script on the planner's tree). The computer list and the Fix a Workstation group are
the window's children laid over the Workstation group (main's arrangement); the planner shrank the group around its own controls, so the
list and the Fix group covered its note (whose third line was cut) and Delete. The Workstation group around them again: Current Computer,
the note on three lines at the group's width, the ComputerName caption 2 px over the list, the list and the Fix group side by side 8 px
apart (the Fix group to the content edge, its content centred as before), Delete 8 px under the list, the group 8 px under Delete."""


def layout(h):
    g2 = h.n("groupBox2")
    W = g2.nw
    h.set("label1", 8, 50, W - 16, 48)
    h.set("label2", 8, 102, None, 18)
    top = g2.ny + 102 + 18 + 2
    lst = h.n("listComputer")
    x = g2.nx + 8
    ht = h.n("groupBox1").nh
    h.set("listComputer", x, top, lst.nw, ht)
    fx = x + lst.nw + 8
    right = g2.nx + W - 8
    g1 = h.n("groupBox1")
    dx = ((right - fx) - g1.nw) // 2
    for name in ("label3", "butSetSimpleGraphics", "label4", "butResetZoom"):
        h.set(name, h.n(name).nx + dx, None)
    h.set("groupBox1", fx, top, right - fx, ht)
    yd = top + ht + 8 - g2.ny
    h.set("butDelete", 8, yd)
    gh = yd + h.n("butDelete").nh + 8
    h.set("groupBox2", None, None, None, gh)
    h.set(".", None, None, None, max(h.tree.nh, g2.ny + gh + 8))
