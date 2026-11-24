"""FormCentralUserGroupEdit on the grid (UI-12 slice 12, layout script on the planner's tree). The label from the padding, the box to the
right padding (it ended 54 px short of Cancel); Delete on the left of the bar, OK and Cancel on the right."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - s.PAD
    fx = s.col_x(h, ["label1"])
    s.row(h, "label1", "textDescription", s.PAD, fx, R - fx)
    s.bar(h, left=("butDelete",), right=("butOK", "butCancel"), bh=24)
