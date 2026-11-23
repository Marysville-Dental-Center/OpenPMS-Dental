"""FormOrthoHardwareAdd on the grid (UI-12 slice 7, layout script on the planner's tree). The caption stood 39 px from the left over a grid at
12 that ended 4 px short of OK's edge: the caption and the grid on the content edges."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.move(h, "labelComments", x=8)
    s.span(h, "gridMain", x=8, r=s.right(h))
