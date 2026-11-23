"""FormMaintRecordEdit on the grid (UI-12 slice 6, layout script on the planner's tree). The planner grew the window from main's 760 to 1033 px
for the Date done row (the two run-time lines beside it) and put the Note box on the date's field column to 1025: back to main's width with
the item line, the description, then one row of the due line, Date done and the done-by line (the texts the code gives them, grid-runtime-
texts) ending on the right edge, the two grids with their buttons on the 4 px gap, the Note row from the padding to the right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    W = 760
    h.set(".", None, None, W, None)
    R = W - 8
    h.set("labelItem", 8, 8, R - 8, 20)
    h.set("textDescription", 8, 32, R - 8, 50)
    y = 90
    xd = R - 278
    h.set("labelDoneBy", xd, y + 2, 278, None)
    xf = xd - 6 - 100
    h.set("textDateDone", xf, y)
    h.set("labelDateDone", xf - 6 - 68, y)
    h.set("labelDue", 8, y + 2, 250, None)
    y += 23 + 8
    h.set("gridSteps", 8, y, R - 8, None)
    y += 165 + 4
    h.set("butCheckAll", 8, y)
    y += 24 + 8
    h.set("gridFields", 8, y, R - 8, None)
    y += 165 + 4
    h.set("butEnterValue", 8, y)
    y += 24 + 8
    lw = h.lw("labelNote")
    h.set("labelNote", 8, y, lw, None)
    h.set("textNote", 8 + lw + 6, y, R - (8 + lw + 6), None)
    h.set("butSave", R - 75, None)
