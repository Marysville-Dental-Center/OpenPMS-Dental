"""FormSupplyAdjust on the grid (UI-12 slice 9, layout script on the planner's tree): the planner's second pass on its own layout widens the
Change box from 72 to 73 px (its right edge onto the 4 px grid at 188) and moves its note 1 px with it: no fixed point in one pass. The box
at 73 px and the note 6 px after it, where the planner's rules stop."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    h.set("textChange", None, None, 73, None)
    t = h.n("textChange")
    s.move(h, "labelChangeNote", x=t.nx + t.nw + 6)
