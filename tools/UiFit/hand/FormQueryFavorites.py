"""FormQueryFavorites on the grid (UI-12 slice 10, layout script on the planner's tree). The planner moved the split container's two panels
into it (8, 8 and 473, 8: the container places them) and grew the window 1084 x 656 -> 1129 x 684 (the code sets its width, 500 or 1100 px,
LayoutManager.Scale): main's size and panels. In the left panel (the code sets SplitterDistance 454) the search row, the grid and the button
row end 8 px before its edge, the grid 12 px over the buttons; in the right one Wrap Text and the query box from the padding, the box down
to the buttons' bottom."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 1084, 656)
    y = s.bar(h, right_=("butOK",))
    R = s.right(h)
    h.set("splitContainer", 8, 8, R - 8, y - 16)
    H = h.n("splitContainer").nh
    h.set("splitterPanel1", 0, 0, 454, H)
    h.set("splitterPanel2", 455, 0, h.n("splitContainer").nw - 455, H)
    r1 = 454 - 8
    yb = H - 8 - 24
    h.set("label5", 8, 8, None, 25)
    s.move(h, "butShowHide", x=r1 - h.n("butShowHide").nw, y=8)
    h.set("textSearch", 8, 37, r1 - 8, None)
    s.row_left(h, ("butDelete",), 8, yb)
    s.row_right(h, ("butEdit", "butAdd"), r1, yb)
    h.set("gridMain", 8, 64, r1 - 8, yb - 12 - 64)
    w2 = h.n("splitterPanel2").nw
    h.set("checkWrapText", 8, 8, None, None)
    h.set("textQuery", 8, 32, w2 - 16, yb + 24 - 32)
