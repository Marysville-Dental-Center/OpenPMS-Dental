"""FormCentralConnectionEdit on the grid (UI-12 slice 12, layout script on the planner's tree). The planner's layout, with the Note box on
the Remote URI box's right edge (it ended 156 px short) and down to the section gap over the bar (a 29 px band), and Delete, OK and Cancel
24 px tall."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    yb = s.bar(h, left=("butDelete",), right=("butOK", "butCancel"), bh=24)
    g2 = h.n("groupBox2")
    u = h.n("textServiceURI")
    right = g2.nx + u.nx + u.nw
    n = h.n("textNote")
    h.set("textNote", None, None, right - n.nx, yb - s.SECTION - n.ny)
