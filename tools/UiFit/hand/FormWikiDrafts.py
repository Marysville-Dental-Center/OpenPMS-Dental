"""FormWikiDrafts on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the window 1154 -> 1157 wide. Main's
size; the grid, the line numbers, the draft text and the page view from the padding (the code lays the last three out from its working area:
now from the numbers' box, whose right 18 px the text box covers as on main, to 8 px before the button column, UiGrid.PAD from the top and
the bottom); the buttons on the right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 1154, 638)
    R = s.right(h)
    B = s.bottom(h)
    w = h.n("butEdit").nw
    s.column(h, ("butEdit", "butDelete", "butCompare"), R - w, 8)
    h.set("labelNotAuthorized", R - h.n("labelNotAuthorized").nw, 8 + 3 * 24 + 2 * 4 + 8, None, None)
    h.set("gridMain", 8, 8, None, B - 8)
    h.set("textNumbers", 8 + h.n("gridMain").nw + 6, 8, None, B - 8)
    n = h.n("textNumbers")
    left = n.nx + n.nw - 18
    area = R - w - 8 - left
    tw = area // 2 - 2
    h.set("textContent", left, 8, tw, B - 8)
    h.set("webViewWiki", left + area // 2 + 2, 8, tw, B - 8)
