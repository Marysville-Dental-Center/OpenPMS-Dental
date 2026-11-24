"""ControlFamily (the Family module) on the grid (UI-12 slice 14, layout script), with the other modules' numbers (ControlTreat,
ControlAccount: the parts against the module's edges, 4 px under the toolbar, 4 px between the parts). Its code lays the grids out at run
time: the recall grid to the module's right edge (FillGridRecall), the insurance grid beside the super family and clones pane when that is
shown, else in its place, to the right edge (LayoutSuperClones, UiGrid.ROW_GAP after the pane; main had 2 px). Main: the picture 1 px off
the edge, the top row 2 px under the toolbar, the parts 2 px apart. The designer now has the parts where the code puts them with no super
family (the pane hidden by RefreshModuleScreen; the harness hides it too): the picture and the Family Members and Recall grids one row 4 px
under the toolbar, the Patient Information grid, the pane and the insurance grid 4 px under them down to the module's bottom."""

GAP = 4
TOOLBAR = 25


def layout(h):
    W, HH = h.tree.nw, h.tree.nh
    y0 = TOOLBAR + GAP
    h.set("pictureBoxPat", 0, y0, 100, 100)
    xf = 100 + GAP
    h.set("gridFamily", xf, y0, 480, 100)
    xr = xf + 480 + GAP
    h.set("gridRecall", xr, y0, W - xr, 100)
    y1 = y0 + 100 + GAP
    h.set("gridPat", 0, y1, 252, HH - y1)
    xs = 252 + GAP
    h.set("splitContainerSuperClones", xs, y1, 329, HH - y1)
    h.set("gridIns", xs, y1, W - xs, HH - y1, why="in the pane's place (no super family), to the module's right edge, as LayoutSuperClones puts it")
