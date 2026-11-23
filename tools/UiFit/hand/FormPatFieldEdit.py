"""FormPatFieldEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The field's caption and box stood at x 19 with a
37 px band under the box: the caption and box from the padding to the right padding, the box down to 8 px over the bottom bar (Auto
Note, hidden in the designer, on its left)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_W = 375


def layout(h):
    s.size(h, MAIN_W, None)
    R = MAIN_W - 8
    h.set("labelName", 8, None, R - 8, None)
    h.set("textValue", 8, None, R - 8, None)
    yb = s.H(h) - 8 - h.n("butSave").nh
    tv = h.n("textValue")
    h.set("textValue", None, None, None, yb - 8 - tv.ny)
    s.bar(h, left=("butUseAutoNote",), right=("butSave",))
