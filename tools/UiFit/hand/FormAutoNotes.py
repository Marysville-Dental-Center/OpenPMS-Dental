"""FormAutoNotes on the grid (UI-12 slice 7, layout script on the planner's tree). The tree stood 15 px from the left, 18 px before the
button column and 26 px over the bottom: on the content edge, to 8 px before the buttons and 8 px over the bottom."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "treeNotes", x=8, r=h.n("butAdd").nx - 8, b=s.bottom(h))
