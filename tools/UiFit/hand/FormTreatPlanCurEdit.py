"""FormTreatPlanCurEdit on the grid (UI-12 slice 6, layout script on the planner's tree). The grids kept main's 12 px margin, 6 px from the
arrows and 15 px over the bottom row: one width from the padding to the right edge with the arrows 8 px from each and centred on them, down
to 8 px over the bottom row; Heading and Note ending on the left grid's edge; Make Active Treatment Plan right-aligned under that grid."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    top = h.n("gridTP").ny
    bot = h.n("butSave").ny - 8
    gw = (R - 8 - 35 - 16) // 2
    h.set("gridTP", 8, top, gw, bot - top)
    xa = 8 + gw + 8
    h.set("gridAll", xa + 35 + 8, top, R - (xa + 35 + 8), bot - top)
    mid = top + (bot - top) // 2
    h.set("butLeft", xa, mid - 28)
    h.set("butRight", xa, mid + 4)
    for name in ("textHeading", "textNote"):
        n = h.n(name)
        h.set(name, None, None, 8 + gw - n.nx, None)
    h.set("butMakeActive", 8 + gw - 164, None)
