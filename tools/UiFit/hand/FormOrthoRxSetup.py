"""FormOrthoRxSetup on the grid (UI-12 slice 7, layout script on the planner's tree). The note and Add stood 22 px right of the grid's left
edge and Down ended 11 px short of its right edge: the note and Add on the grid's x, Up and Down ending on its right edge; the grid down to its side margin (12 px) over the bottom (it stood 40 px over it)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    g = h.n("gridMain")
    for n in ("label4", "butAdd"):
        s.move(h, n, x=g.nx)
    s.row_right(h, ("butUp", "butDown"), g.nx + g.nw)
    s.span(h, "gridMain", b=h.tree.nh - g.nx)
