"""FormFeeSchedGroups on the grid (UI-12 slice 6, layout script on the planner's tree). The filter group and the grids stood at 10, the grids 2
px apart at the top and bottom, Add Group 22 px over the bottom: the filter group and the Groups grid one width from the padding, the
Clinics grid 8 px after it to the right edge, both on one top 8 px under the group and down to 8 px over Add Group, which stands 8 px over
the bottom."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    B = e.bottom_edge(h)
    gw = (R - 8 - 8) // 2
    h.set("groupFilters", 8, 8, gw, None)
    top = 8 + h.n("groupFilters").nh + 8
    h.set("butAdd", 8, B - 23)
    h.set("gridGroups", 8, top, gw, B - 23 - 8 - top)
    h.set("gridClinics", 8 + gw + 8, top, R - (8 + gw + 8), B - 23 - 8 - top)
