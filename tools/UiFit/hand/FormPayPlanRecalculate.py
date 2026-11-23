"""FormPayPlanRecalculate on the grid (UI-12 slice 6, layout script on the planner's tree). The content kept main's 12 px margin and the group
ended 89 px short of Save: the note, the group and the check box from the padding, the note and the group to the right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    h.set("label1", 8, None, R - 8, None)
    h.set("groupBox1", 8, None, R - 8, None)
    h.set("checkRecalculateInterest", 8, None)
