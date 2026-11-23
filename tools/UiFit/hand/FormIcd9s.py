"""FormIcd9s on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 690 to 721 px and
kept the list at x 20, 85 px short of the right padding: main's height; the search row from the padding; the list from the padding to
the right padding and down to 8 px over OK."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_H = 690


def layout(h):
    s.size(h, None, MAIN_H)
    yb = s.bar(h, right=("butOK",))
    l1 = h.n("label1")
    h.set("label1", 8, None)
    x = 8 + l1.nw + 6
    h.set("textCode", x, None)
    h.set("butSearch", x + h.n("textCode").nw + 6, None)
    lm = h.n("listMain")
    h.set("listMain", 8, None, s.W(h) - 16, yb - 8 - lm.ny)
