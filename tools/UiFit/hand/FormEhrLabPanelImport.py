"""FormEhrLabPanelImport on the grid (UI-12 slice 5, layout script on the planner's tree). The Info group stood at 12 and ended 8 px over
the lab-order grid's bottom beside it; the raw message box, Change and the grid ended at 883, 880 and 879 (OK at 895): the group at the
content edge down to the grid's bottom; the raw message box, the note, Change (its box 6 px before it) and the grid on the right content
edge."""


def layout(h):
    R = h.tree.nw - 8
    g = h.n("gridMain")
    gb = h.n("groupBox1")
    h.set("groupBox1", 8, None, None, g.ny + g.nh - gb.ny)
    for name in ("textHL7Raw", "label6", "gridMain"):
        n = h.n(name)
        h.set(name, None, None, R - n.nx, None)
    c = h.n("butChangePat")
    h.set("butChangePat", R - c.nw, None)
    p = h.n("textPatName2")
    h.set("textPatName2", None, None, R - c.nw - 6 - p.nx, None)
