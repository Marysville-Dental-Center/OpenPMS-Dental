"""FormImageCatMerge on the grid (UI-12 slice 6, layout script on the planner's tree). The Change buttons ended 19 px short of their groups'
content edge and Merge 2 px short of the groups: on those edges."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    for g, b in (("groupBoxInto", "buttonChangeInto"), ("groupBoxFrom", "buttonChangeFrom")):
        h.set(b, h.n(g).nw - 8 - 75, None)
    gi = h.n("groupBoxInto")
    h.set("butMerge", gi.nx + gi.nw - 75, None)
