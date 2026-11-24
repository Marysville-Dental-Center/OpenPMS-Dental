"""FormProcCodeNew on the grid (UI-12 slice 11, layout script on the planner's tree). The Type list kept main's 15 px margin under its
caption at 8, and Add and Add, then another stood 93 px past the field column's right edge. The list under its caption on the padding;
the two buttons at the button height ending on the combos' right edge, 8 px apart."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    h.set("listType", 8, None)
    c = h.n("comboCategory")
    right = c.nx + c.nw
    ok = h.n("butOK")
    y = ok.ny
    h.set("butOK", right - ok.nw, y, None, 24)
    h.set("butAnother", right - ok.nw - 8 - h.n("butAnother").nw, y, None, 24)
