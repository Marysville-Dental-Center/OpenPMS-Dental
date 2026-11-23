"""FormPayPlanChargeSelection on the grid (UI-12 slice 8, layout script on the planner's tree). The list stood at x 12 (Zero out charges
at 8) with 26 px over the bottom bar: the list from the padding to the right padding and down to 8 px over the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    yb = s.bar(h, left=("butZeroOutCharges",), right=("butOK",))
    h.set("listBoxPayPlanCharges", 8, 8, s.W(h) - 16, yb - 8 - 8)
