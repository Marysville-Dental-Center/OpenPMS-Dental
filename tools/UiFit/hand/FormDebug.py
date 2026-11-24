"""FormDebug (OpenPmsDentalHL7) on the grid (UI-12 slice 12, layout script on the planner's tree). The warning on the first row across the
content; the three rows under it with their labels from the padding and their boxes to the right padding (they ended 120 px short of the
output box); the Output caption 2 px over the output box, which spans the content down to the section gap over Start; Start (its width,
24 px tall) on the bottom right."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - s.PAD
    y = s.note(h, "label1", s.PAD, s.PAD, R - s.PAD) + s.ROWGAP
    fx = s.col_x(h, ["label4", "label2", "label3"])
    yb = s.rows(h, [("label4", "textHL7Name"), ("label2", "textServerName"), ("label3", "textDatabaseName")], y, fx, right=R)
    yt = s.bar(h, right=("butStart",), bh=24)
    oy = yb + s.GAP
    h.set("label5", s.PAD, oy, h.lw("label5"), 17)
    h.text_align("label5", "BottomLeft")
    top = oy + 17 + s.CAPGAP
    h.set("textOutput", s.PAD, top, R - s.PAD, yt - s.SECTION - top)
