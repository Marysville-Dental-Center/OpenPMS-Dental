"""FormDashboardWidgets on the grid (UI-12 slice 7, layout script on the planner's tree). Fix round (the lead's review of 2026-11-23): the
planner moved OK to the corner (548) and left the grid at 12..537: the grid from the content edge to OK's right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    ok = h.n("butOK")
    s.span(h, "gridMain", x=8, r=ok.nx + ok.nw)
