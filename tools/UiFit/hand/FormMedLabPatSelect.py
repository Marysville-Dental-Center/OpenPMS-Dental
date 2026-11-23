"""FormMedLabPatSelect on the grid (UI-12 slice 6, layout script on the planner's tree). The grids kept main's 12 px margin and ended 4 px
short of OK: from the padding to the right edge. The band over OK is the run-time note labelExistingLab (shown when the results update an
existing lab)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    for name in ("gridPidInfo", "gridLabs"):
        h.set(name, 8, None, R - 8, None)
