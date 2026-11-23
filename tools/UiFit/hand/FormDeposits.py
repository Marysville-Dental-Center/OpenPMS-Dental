"""FormDeposits on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 18 px from the left: on the content edge (its
right edge kept); the clinic picker's box on the grid's right edge (it stood 86 px short of it)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "grid", x=8)
    g = h.n("grid")
    c = h.n("comboClinics")
    s.move(h, "comboClinics", x=g.nx + g.nw - c.nw)
