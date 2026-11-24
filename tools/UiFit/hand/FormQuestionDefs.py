"""FormQuestionDefs on the grid (UI-12 slice 10, layout script on the planner's tree). The note and the grid stood 15 px from the left, the
grid 24 px over the bottom with Add, Up and Down 19 px after it: on the padding, the button column on the right padding ending on the grid's
bottom (they are anchored to the bottom), the grid 8 px before it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    B = s.bottom(h)
    h.set("label1", 8, None, R - 8, None)
    w = h.n("butAdd").nw
    x = R - w
    y = B - 3 * 26 - 2 * 4
    s.column(h, ("butAdd", "butUp", "butDown"), x, y)
    s.fill(h, "gridMain", r=x - 8, b=B)
