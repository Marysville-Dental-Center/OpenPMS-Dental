"""FormSheetDef on the grid (UI-12 slice 5, layout script on the planner's tree). The Sheet Font Size Reduction group stood 37 px under
the Default Font group beside it: both on one top and one height (the taller one's), the rows under them (Width, Height, Landscape, Use
Mobile Layout and the hidden Imaging rows the code shows) moved down by what that adds, Save and the window with them.

Fix round 2: the Sheet Font Size Reduction group stood 20 px after the Default Font group (the grid's gap between groups is 8): the
Default Font group widened to end 8 px before it (its right edge then on the Description box's), its font name box to its content edge."""


def layout(h):
    g1, g2 = h.n("groupBox1"), h.n("groupBox2")
    ht = max(g1.nh, g2.nh)
    dy = (g1.ny + ht) - (g1.ny + g1.nh)
    h.set("groupBox2", None, g1.ny, None, ht)
    h.set("groupBox1", None, None, None, ht)
    for name in ("label7", "textWidth", "label8", "textHeight", "checkIsLandscape", "checkHasMobileLayout", "checkAutoSaveCheck",
                 "labelAutoSaveOverride", "comboAutoSaveOverride", "butSave"):
        h.set(name, None, h.n(name).ny + dy)
    h.set(".", None, None, None, h.tree.nh + dy)
    w1 = g2.nx - 8 - g1.nx
    h.set("groupBox1", None, None, w1, None)
    fn = h.n("comboFontName")
    h.set("comboFontName", None, None, w1 - 8 - fn.nx, None)
