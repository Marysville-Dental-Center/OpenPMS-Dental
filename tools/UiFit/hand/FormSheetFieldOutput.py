"""FormSheetFieldOutput on the grid (UI-12 slice 5, layout script on the planner's tree). The note, the Field Name caption and the list
stood at 13-15: at the content edge.

Fix round 2: the right column had three right edges (the Mobile Caption and Growth Behavior boxes at 463, the Font group at 470, Save at
474) and stopped short of the content edge: the Font group and the outer boxes to the right content edge (Save's right edge), the group's
own Name and Align boxes 8 px inside it."""


def layout(h):
    for name in ("label1", "label2", "listFields"):
        h.set(name, 8, None)
    R = h.tree.nw - 8
    g = h.n("groupBox1")
    h.set("groupBox1", None, None, R - g.nx, None)
    for name in ("textUiLabelMobile", "comboGrowthBehavior"):
        h.set(name, None, None, R - h.n(name).nx, None)
    for name in ("comboFontName", "comboTextAlign"):
        h.set(name, None, None, (R - g.nx) - 8 - h.n(name).nx, None)
