"""FormGlidewellSetup on the grid (UI-12 slice 6, layout script on the planner's tree). The content kept main's 12 px margin, ending at 582,
568 and 482 against Save at 586: the content from the padding to the right edge, Add on it with the grid 8 px before it; the recorded-by
line (labelBaaConfirmed, the text Load gives it) under the check box's text. The band under the check box is the two run-time lines (who
recorded the agreement, why sending is unavailable)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    e.frame(h)
    R = e.right_edge(h)
    h.set("butAdd", R - 80, None)
    h.set("gridAccounts", 8, None, R - 80 - 8 - 8, None)
    h.set("labelBaaConfirmed", 8 + 16, None, R - 8 - 16, None)
