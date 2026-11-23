"""FormEhrPatListElementEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The Restriction and Operand lists were 75
px wide and cut their items (Medication, GreaterThan): both as wide as the Compare string box (117)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    w = h.n("textCompareString").nw
    h.set("listRestriction", None, None, w, None)
    h.set("listOperand", None, None, w, None)
