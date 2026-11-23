"""BasicTemplate on the grid (UI-12 slice 7, layout script on the planner's tree). The planner moved the split container's two panels to 8, 8
inside it and grew the container around them, so the split line ran past the box: the split container places its panels, so they stay at
0, 0 and 0, 61 in main's 158 x 116 container."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    h.set("splitContainer", None, None, 158, 116)
    h.set("splitterPanel1", 0, 0, 158, 57)
    h.set("splitterPanel2", 0, 61, 158, 55)
