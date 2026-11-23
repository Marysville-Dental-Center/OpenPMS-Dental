"""FormEhrLabPanelImport on the grid (UI-12 slice 5, layout script on the planner's tree). The Info group stood at 12 and ended 8 px over
the lab-order grid's bottom beside it; the raw message box, Change and the grid ended at 883, 880 and 879 (OK at 895): the group from the
content edge to 8 px before the right column and down to the grid's bottom (Patient Name and the Service group to its content edge); the raw message box, the note, Change (its box 6 px before it) and the grid on the right content
edge."""


def layout(h):
    R = h.tree.nw - 8
    g = h.n("gridMain")
    gb = h.n("groupBox1")
    gw = g.nx - 8 - 8
    h.set("groupBox1", 8, None, gw, g.ny + g.nh - gb.ny)
    GR = gw - 8
    n = h.n("textPatName")
    h.set("textPatName", None, None, GR - n.nx, None)
    s = h.n("groupBox2")
    h.set("groupBox2", None, None, GR - s.nx, None)
    t = h.n("textServiceName")
    h.set("textServiceName", None, None, (GR - s.nx) - 8 - t.nx, None)
    for name in ("textHL7Raw", "label6", "gridMain"):
        n = h.n(name)
        h.set(name, None, None, R - n.nx, None)
    c = h.n("butChangePat")
    h.set("butChangePat", R - c.nw, None)
    p = h.n("textPatName2")
    h.set("textPatName2", None, None, R - c.nw - 6 - p.nx, None)
