"""FormLaboratories on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 12 px from the edges and 42 px over Add:
on the content edges down to 8 px over Add, Add on the right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    a = h.n("butAdd")
    s.move(h, "butAdd", x=R - a.nw)
    s.span(h, "gridMain", x=8, r=R, b=a.ny - 8)
