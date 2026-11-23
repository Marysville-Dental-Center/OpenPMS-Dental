"""FormDomainUserPick on the grid (UI-12 slice 7, layout script on the planner's tree). None stood 4 px left of OK's column: on OK's
column, the grid 8 px before it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    ok = h.n("butOK")
    s.move(h, "butNone", x=ok.nx)
    s.span(h, "gridMain", r=ok.nx - 8)
