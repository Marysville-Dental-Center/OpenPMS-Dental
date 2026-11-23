"""FormPopupDisplay on the grid (UI-12 slice 6, layout script on the planner's tree). The description box stood at 53, on neither column: from
the padding to 8 px before the Disable for group (hidden in the designer, shown by the code), which stands on the right edge at the top."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    gx = R - 276
    h.set("groupBox1", gx, 8)
    h.set("textDescription", 8, None, gx - 8 - 8, None)
