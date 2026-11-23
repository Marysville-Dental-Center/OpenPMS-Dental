"""FormGradingScaleEdit on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 12 px from the left against Delete's
8: on the content edge. The note the code shows when the scale is in use (grid-runtime-texts) stood between Delete and Save at 235 px for
its 359 px text on one line: from 6 px after Delete to 6 px before Save, on two lines ending on the buttons' bottom. Fix round (the lead's
review of 2026-11-23): Add on Save's column (it stood 7 px left of it), the grid to 8 px before it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    sv = h.n("butSave")
    s.move(h, "butAdd", x=sv.nx)
    s.span(h, "gridMain", x=8, r=sv.nx - 8)
    d = h.n("butDelete")
    x = d.nx + d.nw + 6
    w = sv.nx - 6 - x
    nh = h.note_h("labelWarning", w)
    h.set("labelWarning", x, d.ny + d.nh - nh, w, nh)
