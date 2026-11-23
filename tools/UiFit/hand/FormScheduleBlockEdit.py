"""FormScheduleBlockEdit on the grid (UI-12 slice 9, layout script on the planner's tree): the Blockout Type list ended 3 px above the Note
box beside it: it ends on the box's bottom."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    n = h.n("textNote")
    s.span(h, "listType", b=n.ny + n.nh)
