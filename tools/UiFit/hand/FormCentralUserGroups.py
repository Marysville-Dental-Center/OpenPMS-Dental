"""FormCentralUserGroups on the grid (UI-12 slice 12, layout script on the planner's tree). The planner grew the window 27 px for main's
12 px margins: back to main's 296 x 380: the list from the padding to the bottom padding, Add at its top and Close on the bottom right in
the column beside it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 296, 380)
    R = 296 - s.PAD
    B = 380 - s.PAD
    bw = h.n("butClose").nw
    h.set("listGroups", s.PAD, s.PAD, R - bw - s.GAP - s.PAD, B - s.PAD)
    h.set("butAddGroup", R - bw, s.PAD, bw, 24)
    h.set("butClose", R - bw, B - 24, bw, 24)
