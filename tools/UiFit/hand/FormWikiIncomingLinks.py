"""FormWikiIncomingLinks on the grid (UI-12 slice 10, layout script on the planner's tree). The grid stood 12 px from the left and 16 over the
bottom, the page view ended 51 px short of the right: on the padding, the view 6 px after the grid (as main) to the right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.fill(h, "gridMain", y=8, r=8 + h.n("gridMain").nw)
    s.fill(h, "webViewWiki", x=8 + h.n("gridMain").nw + 6, y=8)
