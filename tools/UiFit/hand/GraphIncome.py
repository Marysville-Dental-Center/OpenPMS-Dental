"""GraphIncome on the grid (UI-12 slice 12 fix round, layout script on the planner's tree). The planner lays out the window; in the Filter
Dates group it kept the group's width and gave the To date picker what was left (93 px, which draws another drop button than the 112 px
From picker; review finding 10). The two pickers are one width, as on main (113 and 113), and the group as wide as To plus the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    w = h.n("dateTimeFrom").nw
    h.set("dateTimeTo", None, None, w, None)
    # its designer x kept: the built control is scaled a little (its tree x is 4 px less than the designer's), and the tree's x written
    # back would close the gap to View to 4 px
    h.set("groupBox2", h.designer.point("groupBox2")[0], None, h.n("dateTimeTo").nx + w + s.PAD, None)
