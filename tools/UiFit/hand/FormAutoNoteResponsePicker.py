"""FormAutoNoteResponsePicker on the grid (UI-12 slice 7, layout script on the planner's tree). The label stood 16 px and the grid 25 px from
the left: the label on the content edge, its box 6 px after it to the grid's right edge, the grid on the content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = h.n("gridMain").nx + h.n("gridMain").nw
    lw = h.lw("labelResponseText")
    h.set("labelResponseText", 8, None, lw, None)
    s.span(h, "textResponseText", x=8 + lw + 6, r=R)
    s.span(h, "gridMain", x=8)
