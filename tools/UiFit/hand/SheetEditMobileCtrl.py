"""SheetEditMobileCtrl on the grid (UI-12 slice 6, layout script on the planner's tree). The drag note was 240 px for its 249 px text (cut) and
the check box stood against it: the note from the padding as wide as its text, Use Mobile Layout 12 px after it, Order Fields from Desktop
on the right edge with Add Header 6 px before it, centred on one row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    lw = h.lw("labelDragToOrder")
    h.set("labelDragToOrder", 8, 11, lw, None)
    h.check_left("checkUseMobileLayout")
    h.set("checkUseMobileLayout", 8 + lw + 12, 11)
    h.set("butOrderFields", R - 152, 8)
    h.set("butAddHeader", R - 152 - 6 - 75, 8)
