"""FormWikiSearch on the grid (UI-12 slice 6, layout script on the planner's tree). The search box, the limit row and the grid kept main's 12
px margin, the check boxes started 73 px after Search, the browser ended 10 px before Restore's column and Restore 4 px short of the right
edge: the left column from the padding, the check boxes from 16 px after Search 6 px apart, Restore on the right edge, the browser to the
right edge, the grid down to 8 px over OK."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    h.set("textSearch", 8, None, 252, None)
    h.check_left("checkLimit")
    h.set("checkLimit", 8, None)
    h.set("textLimit", 8 + 103 + 6, None)
    x = 266 + 75 + 16
    for name in ("checkIgnoreContent", "checkArchivedOnly", "checkBoxMatchWholeWord", "checkBoxShowMainPages", "checkReportServer"):
        h.set(name, x, None)
        x += h.n(name).nw + 6
    h.set("butRestore", R - 75, None)
    w = h.n("webViewWiki")
    h.set("webViewWiki", None, None, R - w.nx, None)
    g = h.n("gridMain")
    h.set("gridMain", 8, None, 252, h.n("butOK").ny - 8 - g.ny)
