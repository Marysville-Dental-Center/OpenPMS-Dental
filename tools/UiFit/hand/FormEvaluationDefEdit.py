"""FormEvaluationDefEdit on the grid (UI-12 slice 7, layout script on the planner's tree). Total Points stood 2 px before its box and the
grid 12 px from the left against Delete's 8: the label 6 px before its box on its row, the grid on the content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.label_before(h, "labelTotalPoint", "textTotalPoints")
    s.span(h, "gridMain", x=8)
