"""FormClaimAttachPasteDXCItem on the grid (UI-12 slice 8, layout script on the planner's tree). A 66 px empty band stood between the
Image Type list and the bottom bar (the window keeps main's size): the list grows down to 8 px over the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    yb = s.bar(h, left=("butDelete",), right=("butSave",))
    lb = h.n("listBoxImageType")
    h.set("listBoxImageType", None, None, None, yb - 8 - lb.ny)
