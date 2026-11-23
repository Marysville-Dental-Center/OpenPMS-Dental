"""FormEmailAddresses on the grid (UI-12 slice 6, layout script on the planner's tree). The content kept main's 16 px margin and Add was
narrower than the group over it: the content from the padding, the group on the right edge with Add as wide as it under it, the grid 8 px
before them, the interval row following its box."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    gx = R - 106
    h.set("groupEmailPrefs", gx, None)
    h.set("butAdd", gx, None, 106, None)
    h.set("label2", 8, None)
    g = h.n("gridMain")
    h.set("gridMain", 8, None, gx - 8 - 8, None)
    h.set("labelInboxCheckInterval", 8, None)
    h.set("textInboxCheckInterval", 8, None)
    h.set("labelInboxCheckUnits", 8 + 32 + 6, None)
    h.set("checkEmailDisclaimer", 8 + 32 + 6 + 102 + 12, None)
