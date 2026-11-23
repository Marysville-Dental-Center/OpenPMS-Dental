"""DashIndividualDiscount on the grid (UI-12 slice 6 fix round, layout script on the planner's tree). The planner put the Discount Plan
group at 8, 8 inside the user control and grew the control to 190 x 124 (8 px around the group), while its hosts give it a fixed size
(ControlAccount's groupBoxIndDis and ControlTreat's userControlIndDis, main's 170 x 84 and 173 x 84): the group at 0, 0 like the sibling
summaries (DashFamilyInsurance, DashIndividualInsurance) and the control as large as the group (174 x 108); the hosts take this size
(ControlTreat's layout script reads it from this designer, ControlAccount's hand row in grid-hand/pilot.tsv)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    g = h.n("groupBoxIndDiscount")
    h.set("groupBoxIndDiscount", 0, 0)
    h.set(".", None, None, g.nw, g.nh)
    e.icon_buttons(h)
