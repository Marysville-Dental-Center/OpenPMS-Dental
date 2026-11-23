"""FormOrthoHardwareSpecs on the grid (UI-12 slice 7, layout script on the planner's tree). The Type caption and list stood 24 px from the left
and 18 px before the grid, Up and Down ended 65 px short of the grid's edge: the caption and the list on the content edge, the grid 8 px
after the list to the right edge, Add on the grid's x, Up and Down ending on its right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    h.set("label3", 8, 8, None, 17)
    h.text_align("label3", "BottomLeft")
    lst = h.n("listType")
    h.set("listType", 8, 27, None, None)
    X = 8 + lst.nw + 8
    s.span(h, "gridMain", x=X, r=R)
    s.move(h, "butAdd", x=X)
    s.row_right(h, ("butUp", "butDown"), R)
