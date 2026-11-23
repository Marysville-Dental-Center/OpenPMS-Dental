"""FormProvStudentBulkEdit on the grid (UI-12 slice 6, layout script on the planner's tree). The grid kept main's 12 px margin, 4 px from the
right column and 27 px over the bottom: from the padding to 8 px before the column, down to the bottom padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    g = h.n("gridStudents")
    h.set("gridStudents", 8, None, h.n("comboClass").nx - 8 - 8, e.bottom_edge(h) - g.ny)
