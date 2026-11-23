"""FormCdsTriggerEdit on the grid (UI-12 slice 5, layout script on the planner's tree). The button groups stood at 12, the conditions grid
ended 80 px over Instructions (an empty band beside the lower groups) and 3 px short of Save's edge: the groups at the content edge; the
grid, the Cardinality note, Instructions and Bibliography to the right content edge; the grid down to 8 px over Instructions."""


def layout(h):
    R = h.tree.nw - 8
    for name in ("groupBox1", "groupBox2", "groupBox3", "groupBox4", "groupBox5", "groupBox6"):
        h.set(name, 8, None)
    g = h.n("gridMain")
    ti = h.n("textInstruction")
    h.set("gridMain", None, None, R - g.nx, ti.ny - 8 - g.ny)
    for name in ("labelCardinality", "textInstruction", "textBibliography"):
        n = h.n(name)
        h.set(name, None, None, R - n.nx, None)
