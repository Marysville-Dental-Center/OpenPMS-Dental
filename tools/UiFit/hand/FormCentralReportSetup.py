"""FormCentralReportSetup on the grid (UI-12 slice 4, layout script on the planner's tree). The tab control stood at 6, 4 with 3 px at the
right: at the content edge (8, 8) to 8 px from the right edge and 8 px over the OK and Cancel row; the report page (UserControlReportSetup,
its own layout script) keeps its place in its tab page."""


def layout(h):
    root = h.tree
    ok = h.n("butOK")
    h.set("tabControl1", 8, 8, root.w - 16, ok.ny - 8 - 8)
