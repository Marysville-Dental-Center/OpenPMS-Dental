"""FormInnoDb on the grid (UI-12 slice 6, layout script on the planner's tree). The content kept main's 14 px margin with the buttons 20 px
over the bottom: the content from the padding to the right edge, the buttons 8 px over the bottom, the text box down to 8 px over them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    B = e.bottom_edge(h)
    h.set("label1", 8, None)
    h.set("butToMyIsam", 8, B - 24)
    h.set("butToInnoDb", 8 + 108 + 6, B - 24)
    t = h.n("textBox1")
    h.set("textBox1", 8, None, R - 8, B - 24 - 8 - t.ny)
