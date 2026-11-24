"""FormXDRSetup on the grid (UI-12 slice 11, layout script on the planner's tree). The planner kept the Clinic / Location ID group at main's
65 px, neither on the padding nor on a column. The group from the padding to the field column's right edge + its padding (its labels and
fields stay on the window's label and field columns); Import and Clear on the bottom bar's line at its height."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    yb = s.bar(h, right=("butSave",))
    g = h.n("groupBox2")
    dx = g.nx - 8
    s.shift_children(h, "groupBox2", dx, 0)
    right = h.n("textPath").nx + h.n("textPath").nw + 8
    h.set("groupBox2", 8, None, right - 8, None)
    bh = h.n("butSave").nh
    x = h.n("textPath").nx
    h.set("butImport", x, yb, None, bh)
    h.set("butClear", x + h.n("butImport").nw + 8, yb, None, bh)
