"""FormEduResourceEdit on the grid (UI-12 slice 11, layout script on the planner's tree). The Lab Results group put its fields at x 124
while the Problems group and the rows between the groups use x 175: the window's one field column. Its labels right-aligned before them,
Test Name ending on the Problem boxes' right edge, the example after Compare Value."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    x = h.n("textProblem").nx
    right = x + h.n("textProblem").nw
    for name in ("textLabResultsID", "textLabTestName", "textCompareValue"):
        h.set(name, x, None)
    h.set("textLabTestName", None, None, right - x, None)
    for label, field in (("label3", "textLabResultsID"), ("label5", "textLabTestName"), ("labelCriterionValue", "textCompareValue")):
        s.label_before(h, label, field)
    c = h.n("textCompareValue")
    h.set("label6", c.nx + c.nw + 6, None)
