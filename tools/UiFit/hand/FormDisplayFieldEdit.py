"""FormDisplayFieldEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 154
to 185 px (Save 12 px under the last row) and left Internal Name 7 px wider than the Description boxes: main's height, Save on the bottom
and right padding (beside the Column Width row, which ends at x 213), Internal Name on the Description boxes' right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_H = 154


def layout(h):
    s.size(h, None, MAIN_H)
    s.bar(h, right=("butSave",))
    d = h.n("textDescription")
    h.set("textInternalName", None, None, d.nw, None)
