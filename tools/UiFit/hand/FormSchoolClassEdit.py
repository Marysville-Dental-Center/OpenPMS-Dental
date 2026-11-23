"""FormSchoolClassEdit on the grid (UI-12 slice 9, layout script on the planner's tree): Delete stood at 50 against the bar's 8 px: at the
padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.move(h, "butDelete", x=8)
