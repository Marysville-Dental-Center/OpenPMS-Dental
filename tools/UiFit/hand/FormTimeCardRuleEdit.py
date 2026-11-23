"""FormTimeCardRuleEdit on the grid (UI-12 slice 6, layout script on the planner's tree). One check box stood at the padding and the other on
the field column, the Rate2 group at 31 with its fields at 159 in it, the Employee list at 31: the label + field rows on one field column
(190), the Rate2 group from the padding to the right edge with its fields on that column (6 AM and 5 PM 6 px after them), the two check
boxes a list at the padding, the Employee caption (the text the code gives it) and list from the padding to the right edge down to 8 px over
the bottom row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    F = 190
    g = h.n("groupBox2")
    h.set("groupBox2", 8, 39, R - 8, None)
    fg = F - 8
    for lab, fld, but in (("label4", "textBeforeTimeOfDay", "but6am"), ("label3", "textAfterTimeOfDay", "but5pm")):
        h.set(fld, fg, None)
        h.set(but, fg + 64 + 6, None)
        h.set(lab, fg - 6 - h.n(lab).nw, None)
    y = 39 + g.nh + 8
    for name in ("checkUseRate3", "checkIsOvertimeExempt"):
        h.check_left(name)
        h.set(name, 8, y, h.cw(name), 20)
        y += 22
    y += 4
    h.set("textClockInMin", F, y)
    h.set("label5", F - 6 - h.n("label5").nw, y)
    y += 23 + 8
    h.set("label1", 8, y, R - 8, 17)
    y += 17 + 2
    h.set("listEmp", 8, y, R - 8, h.n("butSave").ny - 8 - y)
