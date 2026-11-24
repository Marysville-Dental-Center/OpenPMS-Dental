"""FormCvxs on the grid (UI-12 slice 11, layout script on the planner's tree). The planner kept main's 20 px left margin and ended the grid
82 px short of the window's right padding. The search row on the padding (caption, box, Search); the grid under it from the padding to the
right padding, down to the section gap over OK."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    yb = s.bar(h, right=("butOK",))
    lw = h.lw("label1")
    h.text_align("label1", "MiddleRight")
    h.set("label1", 8, 8, lw, h.H)
    t = h.n("textCode")
    h.set("textCode", 8 + lw + 6, 8, t.nw, h.H)
    h.set("butSearch", 8 + lw + 6 + t.nw + 6, 8, None, h.H)
    top = 8 + h.H + 8
    h.set("gridMain", 8, top, R - 8, yb - s.SECTION - top)
