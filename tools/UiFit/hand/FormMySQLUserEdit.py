"""FormMySQLUserEdit on the grid (UI-12 slice 12, layout script on the planner's tree). The three rows with their labels from the padding
and their boxes to the right padding (they ended 40 px short of Cancel); the Permission Level group on the field column under them at the
sibling gap; OK and Cancel 24 px tall on the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - s.PAD
    fx = s.col_x(h, ["label3", "label5", "label6"])
    yb = s.rows(h, [("label3", "textUser"), ("label5", "textPassword"), ("label6", "textRetypePassword")], s.PAD, fx, right=R)
    h.set("groupBox1", fx, yb + s.GAP)
    s.bar(h, right=("butOK", "butCancel"), bh=24)
