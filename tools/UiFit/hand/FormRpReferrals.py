"""FormRpReferrals on the grid (UI-12 slice 8, layout script on the planner's tree). The tab control and the SQL box stood at x 14 (the
box 128 px short of the right padding): both from the padding to the right padding, the SQL box down to 8 px over OK."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    yb = s.bar(h, right=("butOK",))
    h.set("tabReferrals", 8, None, R - 8, None)
    ts = h.n("textSQL")
    h.set("textSQL", 8, None, R - 8, yb - 8 - ts.ny)
