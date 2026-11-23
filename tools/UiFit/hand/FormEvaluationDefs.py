"""FormEvaluationDefs on the grid (UI-12 slice 7, layout script on the planner's tree). The planner has no fixed point here (the Course combo
box 1 px wider each pass) and left it 3 px past the buttons under it: the combo box on the buttons' right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    a = h.n("butAdd")
    s.span(h, "comboCourse", r=a.nx + a.nw)
