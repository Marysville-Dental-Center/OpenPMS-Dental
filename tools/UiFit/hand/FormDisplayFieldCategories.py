"""FormDisplayFieldCategories on the grid (UI-12 slice 8, layout script on the planner's tree). The caption and the list stood at x 23,
15 px short of the right padding: from the padding to the right padding, the list down to 8 px over OK."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    yb = s.bar(h, right=("butOK",))
    h.set("label1", 8, None, R - 8, None)
    lc = h.n("listCategory")
    h.set("listCategory", 8, None, R - 8, yb - 8 - lc.ny)
