"""FormPerioGraphicalSetup on the grid (UI-12 slice 10, layout script on the planner's tree). The group's right column ended 67 px short of its
content edge, a cramped pair of columns beside an empty band: the right column's colour buttons on the content edge, their labels 6 px
before them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    r = h.n("groupBox2").nw - 8
    for b, l in (("butColorMGJ", "label6"), ("butColorProbing", "label8"), ("butColorProbingRed", "label9")):
        s.move(h, b, x=r - h.n(b).nw)
        s.label_before(h, l, b)
