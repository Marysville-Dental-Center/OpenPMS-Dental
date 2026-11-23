"""FormEvaluationCriterionDefEdit on the grid (UI-12 slice 7, layout script on the planner's tree). The check box stood 5 px right of the
field column, its label on another row height: the box on the field column, the label 6 px before it, centred on it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    F = h.n("textDescript").nx
    s.move(h, "checkIsCategoryName", x=F)
    s.label_before(h, "label3", "checkIsCategoryName")
