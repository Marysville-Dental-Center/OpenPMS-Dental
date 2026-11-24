"""FormCentralPasswordChange on the grid (UI-12 slice 12, layout script on the planner's tree). The note spans the content width and is as
tall as its lines; the two rows under it with their labels from the padding and their boxes to the right padding (they ended 116 px short
of Cancel); OK and Cancel 24 px tall on the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - s.PAD
    y = s.note(h, "label1", s.PAD, s.PAD, R - s.PAD) + s.ROWGAP + 4
    fx = s.col_x(h, ["label2", "label4"])
    s.rows(h, [("label2", "textAccessCode"), ("label4", "textPassword")], y, fx, right=R)
    s.bar(h, right=("butOK", "butCancel"), bh=24)
