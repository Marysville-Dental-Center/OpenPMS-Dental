"""FormSheetFieldChart on the grid (UI-12 slice 9, layout script on the planner's tree): the Chart Type group stood at 88, alone in the
window: at the padding to the right content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W = h.n(".").nw
    s.span(h, "groupBox1", x=8, r=W - 8)
