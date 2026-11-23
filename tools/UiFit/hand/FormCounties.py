"""FormCounties on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 464 to 546 px,
left the list and its caption at x 11 and Add and Remove in the middle of the right side (they are anchored to the bottom right): main's
width; the caption and the list from the padding, the list down to the bottom padding and to 8 px before the button column; Add over
Remove on the bottom and right padding (the window has no bottom bar: it closes by its title bar)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_W = 464


def layout(h):
    s.size(h, MAIN_W, None)
    R = MAIN_W - 8
    rm = h.n("butDelete")
    ad = h.n("butAdd")
    yr = s.H(h) - 8 - rm.nh
    h.set("butDelete", R - rm.nw, yr)
    h.set("butAdd", R - ad.nw, yr - 4 - ad.nh)
    h.set("label1", 8, 8)
    lc = h.n("listCounties")
    h.set("listCounties", 8, None, R - rm.nw - 8 - 8, s.H(h) - 8 - lc.ny)
