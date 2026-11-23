"""FormOrthoChartSetup on the grid (UI-12 slice 7, layout script on the planner's tree). Fix round (the lead's review of 2026-11-23): the
planner left Add, Up and Down 4 px left of Save and the grid and its caption at 9: the column on Save's x, the grid and the caption from the
content edge to 8 px before it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    sv = h.n("butSave")
    for n in ("butAdd", "butUp", "butDown"):
        s.move(h, n, x=sv.nx)
    R = sv.nx - 8
    s.span(h, "gridTabNames", x=8, r=R)
    s.span(h, "labelAvailable", x=8, r=R)
