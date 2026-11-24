"""FormVideo on the grid (UI-12 slice 9, layout script on the planner's tree): Settings stood 34 px before the picture's right edge and
Capture at 3: Settings ends on the picture's edge (main's place), Capture at the padding, the (or Space) note 6 px after it and the code's
status note after that to the right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    p = h.n("pictureBoxCamera")
    R = p.nx + p.nw
    s.move(h, "butSettings", x=R - h.n("butSettings").nw)
    s.move(h, "butCapture", x=8)
    x = 8 + h.n("butCapture").nw + 6
    s.move(h, "label2", x=x)
    x += h.n("label2").nw + 6
    s.span(h, "labelInfo", x=x, r=R)
