"""FormReportsFiltered on the grid (UI-12 slice 9, layout script on the planner's tree): the list stood at 13 with 27 px of window under it:
from the padding to 8 px of every edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W, HT = h.n(".").nw, h.n(".").nh
    h.set("listBoxFavorites", 8, 8, W - 16, HT - 16)
