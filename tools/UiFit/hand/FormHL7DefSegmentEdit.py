"""FormHL7DefSegmentEdit on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 17 px from the left, the grid and
the note box ended 8 px short of Save's edge: the grid on the content edges, the note box to the right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    s.span(h, "gridMain", x=8, r=R)
    s.span(h, "textNote", r=R)
