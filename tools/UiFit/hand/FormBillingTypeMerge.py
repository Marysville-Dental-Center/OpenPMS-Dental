"""FormBillingTypeMerge on the grid (UI-12 slice 8, layout script on the planner's tree). In both groups Change stood 10 px above the text
boxes' row, 24 px tall and 22 px short of the group's content edge; Merge ended 14 px short of the groups' right edge: Change on the boxes'
row at the field height, on the content edge, the Behavior box to 8 px before it; Merge on the groups' right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    for g, but, box in (("groupBoxInto", "butChangeInto", "textItemValueInto"), ("groupBoxFrom", "butChangeFrom", "textItemValueFrom")):
        gn = h.n(g)
        R = gn.nw - 8
        bn = h.n(but)
        tb = h.n(box)
        h.set(but, R - bn.nw, tb.ny, None, tb.nh)
        h.set(box, None, None, R - bn.nw - 8 - tb.nx, None)
    gi = h.n("groupBoxInto")
    m = h.n("butMerge")
    h.set("butMerge", gi.nx + gi.nw - m.nw, s.H(h) - 8 - m.nh)
