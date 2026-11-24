"""FormSheetFieldInput on the grid (UI-12 slice 11, layout script on the planner's tree). The planner left the Auto Note buttons (pick and
clear) one row up, beside Tab Order and 5 px past the field column's right edge, and the field list at main's 13 px margin. The two buttons
after the Auto Note box on its row (6 px after it, 6 px apart, their 24 px icon height); the list and its caption on the padding, the list
down to the section gap over the bottom bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    yb = s.bar(h, left=("butDelete",), right=("butSave",))
    t = h.n("textAutoNote")
    x = t.nx + t.nw + 6
    h.set("butAutoNote", x, t.ny)
    h.set("butClearAutoNote", x + h.n("butAutoNote").nw + 6, t.ny)
    h.set("label2", 8, None)
    h.set("listFields", 8, None, None, yb - s.SECTION - h.n("listFields").ny)
