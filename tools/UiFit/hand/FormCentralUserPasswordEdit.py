"""FormCentralUserPasswordEdit on the grid (UI-12 slice 12, layout script on the planner's tree). The three rows with their labels from the
padding and their boxes to the right padding (they ended 56 px short of Cancel); Show on the field column under them; OK and Cancel on the
bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - s.PAD
    fx = s.col_x(h, ["label3", "labelCurrent", "label1"])
    yb = s.rows(h, [("label3", "textUserName"), ("labelCurrent", "textCurrent"), ("label1", "textPassword")], s.PAD, fx, right=R)
    h.check_left("checkShow")
    h.set("checkShow", fx, yb + s.ROWGAP, h.cw("checkShow"), h.CH)
    s.bar(h, right=("butOK", "butCancel"), bh=24)
