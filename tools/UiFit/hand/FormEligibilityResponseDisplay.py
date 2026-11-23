"""FormEligibilityResponseDisplay on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from
main's 814 to 823 px and left Print Preview 13 px short of the right padding with a 35 px band over it: main's width, Print Preview on
the bottom and right padding, the grid inside the padding down to 8 px over it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_W = 814


def layout(h):
    s.size(h, MAIN_W, None)
    yb = s.bar(h, right=("butPrintPreview",))
    h.set("dataGridView1", 8, 8, MAIN_W - 16, yb - 8 - 8)
