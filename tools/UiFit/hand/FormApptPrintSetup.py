"""FormApptPrintSetup on the grid (UI-12 slice 11, layout script on the planner's tree). The planner left the Color and Orientation groups
at x 53 (on no column), Save floating 48 px over the button row and Preview 1 px in, the routing buttons 13 px in. The two groups on the
field column, one width; Preview on the group's content left, Save and Print ending on its content right, on one row; the routing buttons
on their group's content left."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    g = h.n("groupBox1")
    right = g.nw - 8
    x = h.n("comboStart").nx
    h.set("groupBoxOD2", x, None)
    h.set("groupBoxOD1", x, None)
    y = h.n("butOK").ny
    p = h.n("butPreview")
    h.set("butPreview", 8, y, None, p.nh)
    ok = h.n("butOK")
    h.set("butOK", right - ok.nw, y)
    h.set("butSave", right - ok.nw - 8 - h.n("butSave").nw, y, None, ok.nh)
    h.set("butSelectedDay", 8, None)
    h.set("butSelectedView", 8, None)
