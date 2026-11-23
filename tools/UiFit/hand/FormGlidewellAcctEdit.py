"""FormGlidewellAcctEdit on the grid (UI-12 slice 8 fix round, ui/ui12-8-2; layout script on the planner's tree). The providers caption
and list stood at x 12 and the list ended at 412 while Delete starts at 8 and Save ends at 416: both from the padding to the right padding
(the bar's edges)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    h.set("labelProviders", 8, None, R - 8, None)
    h.set("listProviders", 8, None, R - 8, None)
