"""FormPatFieldDefs on the grid (UI-12 slice 6, layout script on the planner's tree). The content kept main's 15 px margin under the menu and
the buttons ended 11 px short of the right edge: the note, the check box and the grid from the padding, Add, Up and Down on the right edge,
the grid and the note 8 px before them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    bx = R - 79
    for name in ("butAdd", "butUp", "butDown"):
        h.set(name, bx, None)
    h.set("label1", 8, None, bx - 8 - 8, None)
    h.set("checkDisplayRenamed", 8, None)
    h.set("gridMain", 8, None, bx - 8 - 8, None)
