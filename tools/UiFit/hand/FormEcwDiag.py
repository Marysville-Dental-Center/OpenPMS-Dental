"""FormEcwDiag on the grid (UI-12 slice 8, layout script on the planner's tree). The log box stood at x 12 and the Run Check column 4 px
short of the right padding: the column on the right padding, the log box from the padding to 8 px before it and down to the bottom
padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    rc = h.n("butRunCheck")
    xr = R - rc.nw
    h.set("butRunCheck", xr, 8)
    h.check_left("checkShow")
    h.set("checkShow", xr, 8 + rc.nh + 4)
    h.set("textLog", 8, 8, xr - 8 - 8, s.H(h) - 16)
