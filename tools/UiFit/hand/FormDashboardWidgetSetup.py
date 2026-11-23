"""FormDashboardWidgetSetup on the grid (UI-12 slice 6, layout script on the planner's tree). The grids kept main's 12 px margin and tops 1 px
apart, and the top row and Duplicate stood off the grids' edges: the two grids one width, one top and one bottom (8 px over the bottom bar)
from the padding to the right edge with Copy between them 8 px from each; the User Group row starting on the Custom grid's x, Duplicate 8 px
before Save."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    bar = h.n("butSave").ny
    top = 35
    gw = (R - 8 - 75 - 16) // 2
    h.set("gridInternal", 8, top, gw, bar - 8 - top)
    xc = 8 + gw + 8
    h.set("butCopy", xc, top)
    xg = xc + 75 + 8
    h.set("gridCustom", xg, top, R - xg, bar - 8 - top)
    h.set("butTools", xc, None)
    h.set("comboUserGroup", xg, None)
    h.set("labelUserGroup", xg - 6 - h.n("labelUserGroup").nw, None)
    h.set("butSetAll", xg + 184 + 6, None)
    h.set("butAdd", xg + 184 + 6 + 68 + 6, None)
    h.set("butDuplicate", h.n("butSave").nx - 8 - h.n("butDuplicate").nw, None)
