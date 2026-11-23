"""FormLoincs on the grid (UI-12 slice 7, layout script on the planner's tree), as FormCpts: the grid from the content edge to OK's edge
(it ended 85 px short), the search row from the content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "gridMain", x=8, r=s.right(h))
    lw = h.lw("label1")
    h.set("label1", 8, None, lw, None)
    s.row_left(h, ("textCode", "butSearch"), 8 + lw + 6)
