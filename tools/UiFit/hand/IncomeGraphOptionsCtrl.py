"""IncomeGraphOptionsCtrl on the grid (UI-12 slice 12, layout script on the planner's tree). The planner's layout (the two check boxes as a
list), with each check box's text left-aligned after its box (main's text was right-aligned in a wider box)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    for name in ("checkIncludePaySplits", "checkIncludeInsuranceClaimPayments"):
        h.check_left(name)
        h.text_align(name, "MiddleLeft")
