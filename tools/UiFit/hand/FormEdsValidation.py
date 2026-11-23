"""FormEdsValidation on the grid (UI-12 slice 7, layout script on the planner's tree). The header and the grid stood 12 px from the left and
right against the buttons' 8: on the content edges."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    s.span(h, "labelHeader", x=8, r=R)
    s.span(h, "gridMain", x=8, r=R)
