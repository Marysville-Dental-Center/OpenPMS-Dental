"""FormRecallsPat on the grid (UI-12 slice 10, layout script on the planner's tree). The grid stood 12 px from the left, 17 from the right and
29 px over the buttons; Set Perio stood at 263: the grid on the padding and 12 px over the row, Set Perio 6 px after Add."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.bar(h, left=("butAdd", "butPerio"))
    s.fill(h, "gridMain", y=8, over="butAdd")
