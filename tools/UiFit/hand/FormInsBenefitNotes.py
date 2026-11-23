"""FormInsBenefitNotes on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 660 to
686 px and left the notes box 95 px short of the right padding: main's height, the box from the padding to the right padding and down to
8 px over Save."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_H = 660


def layout(h):
    s.size(h, None, MAIN_H)
    yb = s.bar(h, right=("butSave",))
    h.set("textBenefitNotes", 8, 8, s.W(h) - 16, yb - 8 - 8)
