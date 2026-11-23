"""FormEhrEduResourcesPat on the grid (UI-12 slice 6, layout script on the planner's tree). The content kept main's 12 px margins and Delete
stood 35 px over the bottom: the content from the padding to the right edge, Delete 8 px over the bottom, the lower grid down to 4 px over
it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    e.frame(h)
    B = e.bottom_edge(h)
    h.set("butDelete", None, B - 23)
    e.bottom_to(h, "gridProvided", B - 23 - 4)
