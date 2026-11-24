"""FormClaimPrint on the grid (UI-12 slice 16): the claim form preview with the grid's padding on the left, top and bottom; the page
buttons' column on the right: Print bottom right, 8 px from the edges, the page arrows and the page count one row a section gap over it,
ending on Print's right edge; the preview a sibling gap before the column (the window's Layout handler does the same from the buttons)."""
import importlib.util
import os
_spec = importlib.util.spec_from_file_location("_s16", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s16.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W, H = h.tree.w, h.tree.h
    yBar = s.bar_right(h, ["butPrint"], [75])
    yRow = yBar - s.SECTION - 23
    xFwd = W - s.PAD - 18
    h.set("butFwd", xFwd, yRow, 18, 23)
    xLabel = xFwd - s.ITEM - 54
    h.set("labelTotPages", xLabel, yRow, 54, 23)
    xBack = xLabel - s.ITEM - 18
    h.set("butBack", xBack, yRow, 18, 23)
    s.surface(h, "Preview2", right=min(xBack, W - s.PAD - 75) - s.GAP)
