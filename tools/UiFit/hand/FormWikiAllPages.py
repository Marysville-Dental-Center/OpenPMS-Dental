"""FormWikiAllPages on the grid (UI-12 slice 6, layout script on the planner's tree). The grid kept main's 12 px margin and ended 4 px under
the browser, Add and the bracket button 4 px short of OK: the grid from the padding, Add and [[ ]] on the right edge with the browser 8 px
before them, the grid and the browser down to 8 px over OK."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    bot = h.n("butOK").ny - 8
    g = h.n("gridMain")
    h.set("gridMain", 8, None, 252, bot - g.ny)
    for name in ("butAdd", "butBrackets"):
        h.set(name, R - 75, None)
    w = h.n("webViewWiki")
    h.set("webViewWiki", None, None, R - 75 - 8 - w.nx, bot - w.ny)
