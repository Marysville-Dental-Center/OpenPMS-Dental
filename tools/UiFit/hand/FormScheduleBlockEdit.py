"""FormScheduleBlockEdit on the grid (UI-12 slice 9, layout script on the planner's tree): the Blockout Type list ended 3 px above the Note
box beside it: it ends on the box's bottom.
Fix round: the Operatories list (to 328) stood beside a 150 px empty band under Note and Blockout Type, ended 15 px short of Save's
right edge, and the gaps before the two lists were 18 and 16 px. The two lists one top and one height, down to 12 px over the bottom bar,
the Note box down to the same line; Operatories ends on Save's right edge; Note to Blockout Type and Blockout Type to Operatories 16 px."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    sv = h.n("butSave")
    b = sv.ny - 12
    r = sv.nx + sv.nw
    gap = 16
    n = h.n("textNote")
    s.span(h, "textNote", b=b)
    xt = n.nx + n.nw + gap
    t = h.n("listType")
    h.set("listType", xt, None, None, b - t.ny)
    h.set("labelType", xt, None, t.nw, None)
    xo = xt + t.nw + gap
    h.set("listOp", xo, t.ny, r - xo, b - t.ny)
    h.set("labelOp", xo, None, r - xo, None)
