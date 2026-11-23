"""FormOperatories on the grid (UI-12 slice 7, layout script on the planner's tree). The note and the grid stood 20 px from the left: on the
content edge; the grid and the clinic picker to 8 px before the button column (19 px), the grid 8 px over the bottom (20). Fix round (the
lead's review of 2026-11-23): the column on the content's right edge (it stood 10 px short of it) and from the grid's top (it stood 3 px
over it), 4 px apart; the grid and the clinic picker to 8 px before it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.move(h, "label1", x=8)
    R = s.right(h)
    g = h.n("gridMain")
    w = h.n("butAdd").nw
    s.column(h, ("butAdd", "butCombine", "butUp", "butDown"), R - w, g.ny)
    r = R - w - 8
    c = h.n("comboClinic")
    s.move(h, "comboClinic", x=r - c.nw)
    s.span(h, "gridMain", x=8, r=r, b=s.bottom(h))
