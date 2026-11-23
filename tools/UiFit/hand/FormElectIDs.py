"""FormElectIDs on the grid (UI-12 slice 6 fix round, layout script on the planner's tree). Add (anchored to the bottom left) kept main's
x 7 under the grid at the padding: the content's left, right and bottom edges on the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    e.frame(h, bottom=True)
    e.icon_buttons(h)
