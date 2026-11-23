"""FormInsPayFix on the grid (UI-12 slice 8 fix round, ui/ui12-8-2; layout script on the planner's tree). The description box stood at
12..411 and Run ended at 424 (12 and 13 px margins): the box from the padding to the right padding, Run on the right and bottom padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    s.bar(h, right=("butRun",))
    h.set("textBoxDescript", 8, None, R - 8, None)
