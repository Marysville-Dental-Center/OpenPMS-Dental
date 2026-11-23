"""FormSpellCheck on the grid (UI-12 slice 6, layout script on the planner's tree). The grid kept main's 12 px margin, Add and Delete 6 px
after it and the Custom Word column 12 px after them, 5 px short of the right edge: the grid from the padding down to the bottom padding,
Add 8 px after it at the top, Delete (anchored to the bottom) on the grid's bottom edge, the Custom Word caption and box 8 px after them to
the right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    g = h.n("gridMain")
    h.set("gridMain", 8, None, 249, e.bottom_edge(h) - g.ny)
    bx = 8 + 249 + 8
    h.set("butAdd", bx, h.n("textCustom").ny + (23 - h.n("butAdd").nh) // 2)
    h.set("butDelete", bx, e.bottom_edge(h) - h.n("butDelete").nh)
    cx = bx + 75 + 8
    h.set("label4", cx, None, R - cx, None)
    h.set("textCustom", cx, None, R - cx, None)
