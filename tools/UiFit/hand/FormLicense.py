"""FormLicense on the grid (UI-12 slice 8, layout script on the planner's tree). The caption and list stood at x 10, the list ended 98 px
over the bottom padding and the license text 31 px over it (the window keeps main's size): the caption and list on the padding, the list
down to the bottom padding, the text 8 px after the list from the top to the bottom and right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    B = s.H(h) - 8
    h.set("labelLicense", 8, None)
    lb = h.n("listBoxLicense")
    h.set("listBoxLicense", 8, None, None, B - lb.ny)
    x = 8 + lb.nw + 8
    h.set("textLicense", x, 8, R - x, B - 8)
