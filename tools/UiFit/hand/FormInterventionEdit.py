"""FormInterventionEdit on the grid (UI-12 slice 7, layout script on the planner's tree). The planner put the Note label 29 px over its box
and 78 px left of it, the grid 12 px from the left and 4 px short of Save's edge: the Note label 6 px before its box on its first line (as
Date before its box), the box 8 px over the bottom bar on the content's right edge, the grid on the content edges down to 8 px over the
box."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    nb = h.n("textNote")
    yb = h.n("butSave").ny - 8 - nb.nh
    s.span(h, "textNote", r=R, y=yb, b=yb + nb.nh)
    lw = h.lw("label6")
    h.set("label6", nb.nx - 6 - lw, yb, lw, h.H)
    h.text_align("label6", "MiddleRight")
    s.span(h, "gridMain", x=8, r=R, b=yb - 8)
