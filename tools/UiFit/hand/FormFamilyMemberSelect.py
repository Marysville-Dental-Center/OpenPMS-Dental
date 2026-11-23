"""FormFamilyMemberSelect on the grid (UI-12 slice 8, layout script on the planner's tree). The list stood at x 24 with a 63 px empty
band under it (the window keeps main's size): the list from the padding to the right padding and down to 8 px over the bottom bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    yb = s.bar(h, left=("butOther",), right=("butOK",))
    lp = h.n("listPats")
    h.set("listPats", 8, None, s.W(h) - 16, yb - 8 - lp.ny)
