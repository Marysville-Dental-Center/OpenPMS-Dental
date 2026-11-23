"""FormTreatPlanAddProcs on the grid (UI-12 slice 6, layout script on the planner's tree). The content kept main's 12 px margin, Add ended 4 px
past the quick buttons grid, the procedures grid 14 px after it: the plan line (the text the code gives it), the quick buttons grid, the
code row and Search Codes from the padding (Add ending on the grid's edge), the procedures grid 8 px after them to the right edge with
Remove at its x."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    h.set("labelPlan", 8, None, R - 8, None)
    h.set("gridQuick", 8, None, 200, None)
    h.set("labelCode", 8, None)
    h.set("textCode", 8 + 40 + 6, None)
    h.set("butAddCode", 8 + 40 + 6 + 88 + 6, None, 200 - (40 + 6 + 88 + 6), None)
    h.set("butSearch", 8, None, 200, None)
    xg = 8 + 200 + 8
    h.set("gridMain", xg, None, R - xg, None)
    h.set("butRemove", xg, None)
    h.set("labelPriority", xg + 75 + 16, None)
    h.set("comboPriority", xg + 75 + 16 + 52 + 6, None)
    h.set("labelHelp", 8, None)
