"""FormOrthoHardwareEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The Hidden row's check box stood 4 px after
the field column and its label 16 px tall, 2 px off the row: the check box on the field column, the label right-aligned 6 px before it as
tall as the field rows, both on the pitch under Note."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    tn = h.n("textNote")
    y = tn.ny + tn.nh + 4
    l1 = h.n("label1")
    h.set("label1", tn.nx - 6 - l1.nw, y, None, tn.nh)
    h.check_left("checkIsHidden")
    ck = h.n("checkIsHidden")
    h.set("checkIsHidden", tn.nx, y + (tn.nh - ck.nh) // 2)
