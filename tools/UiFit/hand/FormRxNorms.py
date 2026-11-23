"""FormRxNorms on the grid (UI-12 slice 6, layout script on the planner's tree). The grid and None kept main's 26 px margin, 17 px short of OK
and 36 px over the bottom row, the search row from 56: the search row from the padding (each button 6 px after the one before), the grid
from the padding to the right edge down to 8 px over the bottom row, None at the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    lw = h.lw("labelCodeOrDesc")
    h.set("labelCodeOrDesc", 8, None, lw, None)
    x = 8 + lw + 6
    h.set("textCode", x, None)
    x += 100 + 6
    for name in ("butSearch", "butExact", "butClearSearch"):
        h.set(name, x, None)
        x += 75 + 6
    h.set("checkIgnore", x, None)
    h.set("butNone", 8, None)
    g = h.n("gridMain")
    h.set("gridMain", 8, None, R - 8, h.n("butOK").ny - 8 - g.ny)
