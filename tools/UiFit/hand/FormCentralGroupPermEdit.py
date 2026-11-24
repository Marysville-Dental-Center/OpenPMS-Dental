"""FormCentralGroupPermEdit on the grid (UI-12 slice 12, layout script on the planner's tree). The planner kept main's 95 px label margin
and an empty band between Type and the group: Type on the first row with its box to the right padding; the group under it at the sibling
gap, from the padding to the right padding, its Date and Days rows on the window's field column and the example note under them as wide as
the group's content and as tall as its lines; OK and Cancel on the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - s.PAD
    fx = s.col_x(h, ["label10"], x=s.PAD + s.PAD)
    yb = s.row(h, "label10", "textName", s.PAD, fx, R - fx)
    gy = yb + s.GAP
    gfx = fx - s.PAD  # the same column in the group's coordinates
    cr = (R - s.PAD) - s.PAD  # the group's content right edge
    yb = s.rows(h, [("label1", "textDate"), ("label2", "textDays")], s.TITLE, gfx)
    yb = s.note(h, "label3", s.PAD, yb + s.ROWGAP, cr - s.PAD)
    h.set("groupBox1", s.PAD, gy, R - s.PAD, yb + s.PAD)
    s.bar(h, right=("butOK", "butCancel"), bh=24)
