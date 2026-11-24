"""FormImageCatMerge on the grid (UI-12 slice 6, layout script on the planner's tree). The Change buttons ended 19 px short of their groups'
content edge and Merge 2 px short of the groups: on those edges.
UI-12F fix round (the review of G-UI3-ui12f): main's 600 x 249 kept (the final pass grew it 4 px for the bar's section gap while 45 px
stood empty under Merge): Merge on the bottom padding and the room the two groups do not fill shared out as equal gaps between them and
over the bar (_s10.keep_main)."""
import importlib.util
import os

KEEP_BAR_GAP = True

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)
_spec10 = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s10 = importlib.util.module_from_spec(_spec10)
_spec10.loader.exec_module(s10)


def layout(h):
    for g, b in (("groupBoxInto", "buttonChangeInto"), ("groupBoxFrom", "buttonChangeFrom")):
        h.set(b, h.n(g).nw - 8 - 75, None)
    gi = h.n("groupBoxInto")
    h.set("butMerge", gi.nx + gi.nw - 75, None)
    s10.keep_main(h, 600, 249, [("groupBoxInto",), ("groupBoxFrom",)], right_=("butMerge",))
