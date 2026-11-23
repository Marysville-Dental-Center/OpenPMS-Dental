"""FormMaintFieldEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The Minimum and Maximum check boxes (they name
the boxes after them) stood at x 30, their text 18 and 16 px short of the label column's edge, Minimum 2 px above its row's centre: both
at one x with the longer text ending 6 px before the field column, each centred on its row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    tmin = h.n("textMin")
    tmax = h.n("textMax")
    x = tmin.nx - 6 - max(h.n("checkMin").nw, h.n("checkMax").nw)
    for ck, tb in (("checkMin", tmin), ("checkMax", tmax)):
        h.check_left(ck)
        h.set(ck, x, tb.ny + (tb.nh - h.n(ck).nh) // 2)
