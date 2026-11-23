"""FormRpPHRawPop on the grid (UI-12 slice 6, layout script on the planner's tree). The calendars stood at 34 and 57 (main's places) under an
empty band, the adjustment list column at 537: the calendars at their drawn 190 x 176 from the padding at the top with TO between them, the
list column 16 px after the second (its three-line caption, the list 2 px under it as wide as the caption)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    CW = 190
    h.set("date1", 8, 8)
    h.set("labelTO", 8 + CW + 8, 8)
    x2 = 8 + CW + 8 + 28 + 8
    h.set("date2", x2, 8)
    xl = x2 + CW + 16
    h.set("label1", xl, 8)
    h.set("listAdjType", xl, 8 + 47 + 2, 195, None)
