"""FormPatFieldDateEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 272 to
276 px and kept the field's caption and box at x 19: main's width, the caption and box on the padding, the caption to the right padding
(the code gives it the field's name)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_W = 272


def layout(h):
    s.size(h, MAIN_W, None)
    R = MAIN_W - 8
    h.set("labelName", 8, None, R - 8, None)
    h.set("textFieldDate", 8, None, None, None)
    s.bar(h, right=("butSave",))
