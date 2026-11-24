"""FormTrackNextSetup on the grid (UI-12 slice 10, layout script on the planner's tree). The Default Dates group stood 29 px from the left and
51 from the right: from the padding to the right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "groupBox3", x=8, r=s.right(h))
