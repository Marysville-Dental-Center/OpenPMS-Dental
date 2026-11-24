"""FormCentralChooseDatabase on the grid (UI-12 slice 12, layout script on the planner's tree; its designer code is in the window's own
.cs file). The note at the top of the group was cut ("middle tie"): it spans the group's content and is as tall as its lines; each caption
2 px over its box, the boxes to the group's content edge; the group from the padding to the right padding; the check box on the left of
the bar, centred on OK and Cancel."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - s.PAD
    cw = (R - s.PAD) - 2 * s.PAD  # the group's content width
    y = s.note(h, "label6", s.PAD, s.TITLE, cw) + s.ROWGAP
    for cap, box in (("label9", "textURI"), ("label11", "textUser2"), ("label10", "textPassword2")):
        y = h.captioned(cap, box, s.PAD, y, cw) + s.ROWGAP
    h.set("groupServer", s.PAD, s.PAD, R - s.PAD, y - s.ROWGAP + s.PAD)
    s.size(h, s.W(h), s.PAD + h.n("groupServer").nh + s.SECTION + 24 + s.PAD)  # the section gap over the bar
    yb = s.bar(h, right=("butOK", "butCancel"), bh=24)
    h.check_left("checkAutoLogin")
    h.set("checkAutoLogin", s.PAD, yb + (24 - h.CH) // 2, h.cw("checkAutoLogin"), h.CH)
