"""FormRxCombine on the grid (UI-12 slice 10, layout script on the planner's tree). The grid ended 6 px short of the right padding, 27 px over
OK: to it, 12 px over OK."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.fill(h, "gridMain", y=33, over="butOK")
