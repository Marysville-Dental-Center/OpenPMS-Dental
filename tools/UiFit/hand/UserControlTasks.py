"""UserControlTasks (the task lists, in FormTasks and docked in the main window) on the grid (UI-12 slice 14, layout script). Its Resize
(UserControlTasks_Resize) and LayoutTreeAndGrid place the tab strip, the calendar, the tree and the grid at run time; main put them at fixed
coordinates against the control's edges (x 0, the full width), the filter row 2 to 4 px under the toolbar with 20 px boxes, and the tree and
grid edge to edge under the tabs. The code now takes the grid's numbers (UiGrid.PAD, ROW_GAP, FIELD_HEIGHT: the constants TOP_TABS and
TOP_CALENDAR of UserControlTasks.cs), and this script puts the designer where the code puts the controls on the For User tab (no calendar,
the main trunk: an empty tree history, the tree 8 px tall): the filter row a field row 4 px under the toolbar on the 8 px padding (label,
box, Clear 6 px after it, Refresh a band gap further), the tab strip 4 px under it, the tree 4 px under the strip, the grid 4 px under the
tree down to the bottom padding, all 8 px from the sides. The calendar keeps its own size (MonthCalendar; package UI-CAL converts it) at the
code's place under the tabs."""

PAD = 8
GAP = 4
TOOLBAR = 25


def layout(h):
    H = h.H
    W, HH = h.tree.nw, h.tree.nh
    y = TOOLBAR + GAP
    lw = h.lw("label1")
    h.text_align("label1", "MiddleRight")
    h.set("label1", PAD, y, lw, H)
    x = PAD + lw + 6
    h.set("textListFilter", x, y, 132, H)
    x += 132 + 6
    h.set("butClearFilter", x, y, 52, H)
    x += 52 + 16
    h.set("butRefresh", x, y, 68, H)
    ytabs = y + H + GAP
    h.set("tabControl", PAD, ytabs, W - 2 * PAD, 20)
    ycal = ytabs + 20 + GAP
    h.set("monthCalendar", PAD, ycal)
    h.set("tree", PAD, ycal, W - 2 * PAD, 8, why="the main trunk's tree (LayoutTreeAndGrid: no history, 8 px), 4 px under the tab strip")
    ygrid = ycal + 8 + GAP
    h.set("gridMain", PAD, ygrid, W - 2 * PAD, HH - PAD - ygrid)
