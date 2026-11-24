"""FormPayorTypeEdit on the grid (UI-12 slice 9 fix round, layout script on the planner's tree): the Note box stopped 45 px over the bottom
bar: it grows down to 12 px over the bar (the section gap)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "textNote", b=h.n("butSave").ny - 12)
