"""FormCDSILabResult on the grid (UI-12 slice 6 fix round, layout script on the planner's tree). In the Microbiology Results group the
Description box ended 14 px short of the border while the Numeric Results group's units combo ends 8 px from it: the box to the group's
content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    gw = h.n("groupBox1").nw
    t = h.n("textSnomedDescription")
    h.set("textSnomedDescription", None, None, gw - 8 - t.nx, None)
    e.icon_buttons(h)
