"""FormUcums on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the window 785 x 691 -> 785 x 720 and kept the
grid 20 px from the left, 90 px short of the right: main's size, the search row and the grid from the padding, the grid to the right padding
and 12 px over OK."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 785, 691)
    s.bar(h, right_=("butOK",))
    lw = h.lw("label1")
    h.set("label1", 8, 8, lw, 23)
    s.span(h, "textCode", x=8 + lw + 6, r=470)
    s.fill(h, "gridMain", y=35, over="butOK")
