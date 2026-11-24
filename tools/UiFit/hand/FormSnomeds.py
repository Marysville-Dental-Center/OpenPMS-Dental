"""FormSnomeds on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the window 974 x 691 -> 983 x 720 (the grid
kept at 20 px from the left): main's size; the search row and the grid from the padding, the grid 8 px before the hidden one-time tools
group (which the code shows at the grid's top right) and 12 px over OK."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 974, 691)
    s.bar(h, right_=("butOK",))
    R = s.right(h)
    s.move(h, "groupBox1", x=R - h.n("groupBox1").nw, y=35)
    lw = h.lw("label1")
    h.set("label1", 8, 8, lw, 23)
    s.span(h, "textCode", x=8 + lw + 6, r=592)
    s.fill(h, "gridMain", y=35, r=h.n("groupBox1").nx - 8, over="butOK")
