"""FormReqStudentsMany on the grid (UI-12 slice 10, layout script on the planner's tree). The grid stood 15 px from the left and 16 over the
bottom, the Class and Course combos 25 px short of the right: on the padding, the combos with their labels on the right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    for c in ("comboClass", "comboCourse"):
        s.move(h, c, x=R - h.n(c).nw)
    s.label_before(h, "label1", "comboClass")
    s.label_before(h, "label2", "comboCourse")
    s.fill(h, "gridMain", y=8, r=h.n("gridMain").nx + h.n("gridMain").nw)
