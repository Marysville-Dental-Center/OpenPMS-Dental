"""FormImagingDevices on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 32 px from the left and 27 px before
the button column, the computer row 33 px in: the row from the content edge (label, box, the two buttons 6 px apart), the grid from the
content edge to 8 px before the buttons."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    lw = h.lw("label2")
    h.set("label2", 8, None, lw, None)
    s.row_left(h, ("textComputer", "butUnassigned", "butStandardTemplates"), 8 + lw + 6)
    s.span(h, "gridMain", x=8, r=h.n("butAdd").nx - 8)
