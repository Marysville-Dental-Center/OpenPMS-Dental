"""FormGradingScales on the grid (UI-12 slice 7, layout script on the planner's tree). Fix round (the lead's review of 2026-11-23): the planner
left Add 16 px left of OK and the grid at 12: Add on OK's column, the grid from the content edge to 8 px before it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    ok = h.n("butOK")
    s.move(h, "butAdd", x=ok.nx)
    s.span(h, "gridMain", x=8, r=ok.nx - 8)
