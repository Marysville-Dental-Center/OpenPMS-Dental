"""FormInsSpanEdit on the grid (UI-12 slice 8 fix round, ui/ui12-8-2; layout script on the planner's tree). The From Code caption and box
stood at x 13 while Delete starts at 8, and the To Code box ended at 219 while Save ends at 238: the From column on the padding, the To
column 8 px after it ending on the right padding, the two boxes (and their captions) one width."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    w = (R - 8 - 8) // 2
    for cap, box, x in (("label3", "textFrom", 8), ("label2", "textTo", R - w)):
        h.set(cap, x, None, w, None)
        h.set(box, x, None, w, None)
