"""FormFieldDefLink on the grid (UI-12 slice 6, layout script on the planner's tree). The grids stood at 16 and ended 5 px short of the right
edge and 39 px over Save: the note from the padding to the right edge, the grids one width from the padding to the right edge with the
arrows between them 8 px from each and centred on them, down to 8 px over Save."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    h.set("label1", 8, None, R - 8, None)
    s = h.n("butSave")
    top = h.n("gridDisplayed").ny
    gh = s.ny - 8 - top
    gw = (R - 8 - 34 - 16) // 2
    h.set("gridDisplayed", 8, top, gw, gh)
    xa = 8 + gw + 8
    h.set("gridHidden", xa + 34 + 8, top, R - (xa + 34 + 8), gh)
    mid = top + gh // 2
    h.set("butRight", xa, mid - 27)
    h.set("butLeft", xa, mid + 4)
