"""FormSheetFieldExam on the grid (UI-12 slice 9, layout script on the planner's tree): the two lists were 142 px wide, 6 px apart, ending
83 px before OK's edge: one width, 8 px apart, from the padding to OK's right edge, their captions over them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    ok = h.n("butOK")
    R = ok.nx + ok.nw
    w = (R - 8 - 8) // 2
    x2 = R - w
    s.span(h, "label1", x=8, r=R)
    s.span(h, "label2", x=8, r=8 + w)
    s.span(h, "listBoxExamSheets", x=8, r=8 + w)
    s.span(h, "label10", x=x2, r=R)
    s.span(h, "listBoxAvailFields", x=x2, r=R)
