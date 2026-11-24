"""FormWikiHistory on the grid (UI-12 slice 10, layout script on the planner's tree). Compare and Revert stood at 12, 9, the grid 12 px from
the left and the text box 6 px over the grid (the code put the box at the grid's width + 6, as if the grid stood at 0; main too): the
buttons and the lock note one row from the padding, the grid 8 px under it to the bottom padding; the code puts the box 6 px after the
grid's right edge and the page view after it to the right padding (UiGrid)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    B = s.bottom(h)
    h.set("butCompare", 8, 8, None, None)
    h.set("butRevert", 8 + h.n("butCompare").nw + 6, 8, None, None)
    x = h.n("butRevert").nx + h.n("butRevert").nw + 6
    h.set("labelNotAuthorized", x, 8, R - x, 24)
    h.text_align("labelNotAuthorized", "MiddleLeft")
    g = h.set("gridMain", 8, 40, None, B - 40)
    left = 8 + g.nw + 6
    d = (R - left) // 2
    h.set("textContent", left, 40, d, B - 40)
    h.set("webViewWiki", left + d, 40, R - left - d, B - 40)
