"""FormPayTerminalEdit on the grid (UI-12 slice 9, layout script on the planner's tree): the clinic picker's box ended 8 px short of the
text boxes above it: it ends on their right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    t = h.n("textName")
    s.span(h, "comboClinic", r=t.nx + t.nw)
