"""FormSubscribersList on the grid (UI-12 slice 10, layout script on the planner's tree). The grid stood 23 px from the left, 22 from the right
and 28 over the bottom: on the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.fill(h, "gridSubscribers", y=8)
