"""FormOrthoAutoClaims on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 12 px from the left against Generate
Claims' 8 and the clinic picker ended 5 px short of the right edge: the picker on the right edge, the grid on the content edges. The grid ends 12 px over the bottom bar (the
section gap the planner keeps there; it stood 15 to 24 px over it)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    c = h.n("comboClinics")
    s.move(h, "comboClinics", x=R - c.nw)
    s.span(h, "gridMain", x=8, r=R)
    s.span(h, "gridMain", b=h.n("butGenerate").ny - 12)
