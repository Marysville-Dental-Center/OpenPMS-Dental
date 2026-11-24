"""FormCentralConnectionGroups on the grid (UI-12 slice 12, layout script on the planner's tree). The planner grew the window 60 px wide
for main's 12 px margins: back to main's 457 x 421 (a window never shrinks): the default group combo box and its label on the first row,
the grid from the padding under it down to the bottom padding, Add at the grid's top and Close on the bottom right in the column beside
it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 457, 421)
    R = 457 - s.PAD
    B = 421 - s.PAD
    h.set("comboConnectionGroup", s.PAD, s.PAD, None, h.H)
    c = h.n("comboConnectionGroup")
    h.set("label1", c.nx + c.nw + s.LGAP, s.PAD, h.lw("label1"), h.H)
    h.text_align("label1", "MiddleLeft")
    top = s.PAD + h.H + s.GAP
    gr = R - 75 - s.GAP
    h.set("gridMain", s.PAD, top, gr - s.PAD, B - top)
    h.set("butAdd", R - 75, top, 75, 24)
    h.set("butClose", R - 75, B - 24, 75, 24)
