"""FormInsBlueBookRules on the grid (UI-12 slice 7, layout script on the planner's tree). The rules grid and the Allowed Fee Method list stood
2 px right of the group's content edge (the check box and the caption on it): on it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "gridInsBlueBookRules", x=8)
    s.span(h, "listAllowedFeeMethod", x=8)
