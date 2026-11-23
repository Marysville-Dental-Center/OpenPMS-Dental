"""FormGridSelection on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 15 px from the left and 4 px short of
OK's edge: on the content edges."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "gridMain", x=8, r=s.right(h))
