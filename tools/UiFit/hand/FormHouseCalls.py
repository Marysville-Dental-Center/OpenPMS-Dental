"""FormHouseCalls on the grid (UI-12 slice 8, layout script on the planner's tree). The radio button and the Date Range group stood at x
42: on the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    h.set("radioConfirm", 8, None)
    h.set("groupBox2", 8, None)
