"""FormEhrLabNoteEdit on the grid (UI-12 slice 6, layout script on the planner's tree). The grid kept main's 12 px margin, Add ended 4 px short
of Save, and the note "Deletes the entire lab note" stood over Delete at 93: the grid from the padding to 8 px before Add, Add on Save's
edge, the grid 8 px over the bottom row, the note 6 px after Delete."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    h.set("butAddComment", R - 75, None)
    d = h.n("butDelete")
    h.set("gridMain", 8, None, R - 75 - 8 - 8, d.ny - 8 - h.n("gridMain").ny)
    h.set("label57", 8 + 75 + 6, d.ny + (23 - 17) // 2)
