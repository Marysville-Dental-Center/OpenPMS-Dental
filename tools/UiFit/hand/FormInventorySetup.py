"""FormInventorySetup on the grid (UI-12 slice 6, layout script on the planner's tree). The planner put the check box on the label + field
rows' field column (244) over its note at 38: the check box at the padding, its note under its text as tall as its lines, the expiry row 8
px under it from the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    h.check_left("checkEnabled")
    h.set("checkEnabled", 8, 8)
    nh = h.note_h("labelEnabled", R - 24)
    h.set("labelEnabled", 24, 8 + 20 + 2, R - 24, nh)
    y = 8 + 20 + 2 + nh + 8
    h.set("labelExpiryDays", 8, y, 230, None)
    h.set("textExpiryDays", 8 + 230 + 6, y)
    h.set("labelExpiryNote", 8 + 230 + 6 + 48 + 6, y + 2)
