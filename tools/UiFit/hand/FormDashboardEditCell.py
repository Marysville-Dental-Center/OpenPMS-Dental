"""FormDashboardEditCell on the grid (UI-12 slice 12, layout script on the designer; the planner refuses its tree, whose bottom strip the
split container's anchors move 3 px from the designer's). The cell editor fills the first panel (the code puts the graph in it); in the
bottom strip OK and Cancel end on the right padding 8 px apart (Cancel ended 3 px from the edge), the print preview button and the
changes note stay where main has them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W = h.n("splitContainer").nw
    h.set("butCancel", W - s.PAD - 75, None)
    h.set("butOk", W - s.PAD - 75 - s.GAP - 75, None)
