"""FormFeeSchedGroupEdit on the grid (UI-12 slice 6, layout script on the planner's tree). The two grids stood at 18 and ended 14 px short of
Save, 39 px over the bottom row, with Delete 2 px taller than Save: the grids one width from the padding to the right edge with the arrows
between them 8 px from each and centred on them, down to 8 px over the bottom row; Delete on Save's row and height."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    s = h.n("butSave")
    h.set("butDelete", 8, s.ny, None, s.nh)
    top = h.n("gridAvailable").ny
    gh = s.ny - 8 - top
    gw = (R - 8 - 35 - 16) // 2
    h.set("gridAvailable", 8, top, gw, gh)
    xa = 8 + gw + 8
    h.set("gridGroup", xa + 35 + 8, top, R - (xa + 35 + 8), gh)
    mid = top + gh // 2
    h.set("butRight", xa, mid - 30)
    h.set("butLeft", xa, mid + 4)
