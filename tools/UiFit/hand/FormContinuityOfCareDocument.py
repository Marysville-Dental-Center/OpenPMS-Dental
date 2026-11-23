"""FormContinuityOfCareDocument on the grid (UI-12 slice 7, layout script on the planner's tree). Everything stood 14 px from the left and
the instructions box ended 48 px short of the right edge: from the content edge to the right edge, the two buttons 6 px apart."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    s.span(h, "labelTitle", x=8, r=R)
    h.set("labelTitle", None, None, None, h.note_h("labelTitle", R - 8))
    s.row_left(h, ("butShowAndPrint", "butExportToFile"), 8)
    s.span(h, "labelInstructions", x=8, r=R)
    s.span(h, "textInstructions", x=8, r=R)
