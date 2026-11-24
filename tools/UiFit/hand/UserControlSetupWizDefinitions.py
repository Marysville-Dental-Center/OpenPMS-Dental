"""UserControlSetupWizDefinitions on the grid (UI-12 slice 10, layout script on the planner's tree). The category caption and list stood 29 px
from the left, the definitions grid 115 px short of the right, Edit Items' buttons 48 px into the group, the Guidelines caption at 24 beside
a box at 128 (on no column): on the padding, the grid 8 px after the list to the right padding and down to the list's bottom, the buttons
from the group's content edge, Guidelines a caption over its box across the page."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    h.set("label13", 8, 8, None, None)
    h.set("listCategory", 8, 27, None, None)
    l = h.n("listCategory")
    s.span(h, "gridDefs", x=8 + l.nw + 8, r=R, y=8, b=l.ny + l.nh)
    s.row_left(h, ("butAdd", "butHide", "butUp", "butDown"), 8, 23)
    g = h.n("groupEdit")
    y = g.ny + g.nh + 8
    h.set("label14", 8, y, h.lw("label14"), 17)
    h.set("textGuide", 8, y + 19, R - 8, None)
