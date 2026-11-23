"""FormRedundantIndexes on the grid (UI-12 slice 6, layout script on the planner's tree). The content kept main's 12 px margin and the check
box ended 14 px short of the right edge: the content from the padding to the right edge, the check box on it with the description 8 px
before it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    e.frame(h)
    R = e.right_edge(h)
    c = h.n("checkLogAddStatements")
    h.set("checkLogAddStatements", R - c.nw, None)
    h.set("labelDescription", 8, None, R - c.nw - 8 - 8, None)
