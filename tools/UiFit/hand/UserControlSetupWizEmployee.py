"""UserControlSetupWizEmployee on the grid (UI-12 slice 10, layout script on the planner's tree). The grid's top stood 20 px under the notes'
top beside it, the right column 8 px under the padding: the notes column, the grid (8 px after it, to 8 px before the right column and the
bottom padding) and the right column (Add, its notes and Advanced 4 px apart) from one top on the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    h.set("label8", 8, 8, None, None)
    h.set("label3", 8, 8 + h.n("label8").nh + 4, None, None)
    x = h.n("butAdd").nx
    y = 8
    for n in ("butAdd", "label2", "label1", "label4", "butAdvanced"):
        h.set(n, x, y, None, None)
        y += h.n(n).nh + 4
    h.set("pictureAdd", x + h.n("butAdd").nw + 6, 11, None, None)
    s.fill(h, "gridMain", x=8 + h.n("label8").nw + 8, y=8, r=x - 8)
