"""FormCpts on the grid (UI-12 slice 7, layout script on the planner's tree; FormHcpcs, FormIcd10s and FormLoincs the same). The grid
stood 20 px from the left and ended 82 px short of OK's edge, the search row 100 px in: the grid from the content edge to OK's edge, the
search label on the content edge, its box 6 px after it and Search 6 px after the box."""
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
