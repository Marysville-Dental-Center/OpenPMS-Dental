"""FormRpNewPatients on the grid (UI-12 slice 8, layout script on the planner's tree). The Providers caption, list and the two check boxes
stood at x 41 and the Date Range group ended 40 px short of the right padding: the left column on the padding (the address check box to
the right padding); the group on the right padding with the Today's Date box on its date boxes' column."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    for name in ("label1", "listProv", "checkProd"):
        h.set(name, 8, None)
    g = h.n("groupBox2")
    xg = R - g.nw
    h.set("groupBox2", xg, None)
    xf = xg + h.n("textDateFrom").nx
    h.set("textToday", xf, None)
    l4 = h.n("label4")
    h.set("label4", xf - 6 - l4.nw, None)
    ca = h.n("checkAddress")
    h.set("checkAddress", 8, None, R - 8, None)
