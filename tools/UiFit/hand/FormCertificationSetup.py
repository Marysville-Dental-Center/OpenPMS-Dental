"""FormCertificationSetup on the grid (UI-12 slice 7, layout script on the planner's tree). The Categories caption stood 16 px under the
grid's top and its list 9 px from the left, 28 px before the grid: the caption on the content top, the list 2 px under it on the content
edge, the grid 8 px after the list (its right edge kept)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    lst = h.n("listBoxCategories")
    b = lst.ny + lst.nh
    h.set("labelCategories", 8, 8, None, 17)
    h.text_align("labelCategories", "BottomLeft")
    s.span(h, "listBoxCategories", x=8, r=8 + lst.nw, y=27, b=b)
    s.span(h, "gridMain", x=8 + lst.nw + 8)
