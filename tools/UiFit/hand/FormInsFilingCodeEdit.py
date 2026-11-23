"""FormInsFilingCodeEdit on the grid (UI-12 slice 6, layout script on the planner's tree). Delete stood at main's 27 px: at the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    h.set("butDelete", 8, None)
