"""FormAllergySetup on the grid (UI-12 slice 7, layout script on the planner's tree). The planner left the grid 23 px from the left and 31 px
short of the button column, the search box 5 px past the grid's right edge: the grid from the content edge to 8 px before the buttons, the
Search label on the content edge and its box to the grid's right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = h.n("butAdd").nx - 8
    s.span(h, "gridMain", x=8, r=R)
    lw = h.lw("labelSearch")
    h.set("labelSearch", 8, None, lw, None)
    s.span(h, "textSearch", x=8 + lw + 6, r=R)
