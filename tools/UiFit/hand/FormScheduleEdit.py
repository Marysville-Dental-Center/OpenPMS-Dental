"""FormScheduleEdit on the grid (UI-12 slice 10, layout script on the planner's tree). The operatory list and its caption ended 12 px short of
the right padding; the hidden clinic picker's box stood 8 px left of the field column: to the padding, the box on the column."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    s.span(h, "listBoxOps", r=R)
    s.span(h, "labelOps", r=R)
    s.move(h, "comboClinic", x=h.n("comboStart").nx - 35)
