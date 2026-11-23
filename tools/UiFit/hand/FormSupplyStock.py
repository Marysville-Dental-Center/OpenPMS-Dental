"""FormSupplyStock on the grid (UI-12 slice 6, layout script on the planner's tree). The planner widened the window from main's 1084 to 1636 px
(the supply line's 860 px) and left an empty middle between the grids: main's size with the supply line (the text the code gives it) from
the padding to the right edge, the Lots grid from the padding, the ledger grid 8 px after it to the right edge, both down to 8 px over the
bottom row, Show lots without stock under the Lots grid, Adjust at the ledger's x, Close on the right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    W, H = 1084, 472
    h.set(".", None, None, W, H)
    R, B = W - 8, H - 8
    yb = B - 24
    h.set("labelSupply", 8, 8, R - 8, 20)
    h.set("gridLots", 8, 32, 300, yb - 8 - 32)
    h.set("gridLedger", 316, 32, R - 316, yb - 8 - 32)
    h.check_left("checkShowEmptyLots")
    h.set("checkShowEmptyLots", 8, yb + 2)
    h.set("butAdjust", 316, yb)
    h.set("butClose", R - 75, yb)
