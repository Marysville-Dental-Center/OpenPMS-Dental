"""FormEhrTimeSynch on the grid (UI-12 slice 6, layout script on the planner's tree). The explanation stood at 25 while the labels start at 15:
the explanation from the padding, ending on Measure's right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    m = h.n("butRefreshTime")
    h.set("label6", 8, None, m.nx + m.nw - 8, None)
