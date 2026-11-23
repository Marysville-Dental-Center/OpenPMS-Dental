"""FormEmployers on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 570 to 577
px and kept the list at x 18 and the button column 64 px under the list's top and 16 px short of the right padding: main's height; the
button column on the right padding from the list's top, 4 px apart; the list from the padding to 8 px before it and down to 8 px over
OK."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_H = 570


def layout(h):
    s.size(h, None, MAIN_H)
    R = s.W(h) - 8
    yb = s.bar(h, right=("butOK",))
    bw = h.n("butAdd").nw
    y = 8
    for name in ("butAdd", "butDelete", "butEdit", "butCombine"):
        h.set(name, R - bw, y)
        y += h.n(name).nh + 4
    h.set("listEmp", 8, 8, R - bw - 8 - 8, yb - 8 - 8)
