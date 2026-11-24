"""FormCentralPasswordCheck on the grid (UI-12 slice 12, layout script on the planner's tree). The planner kept main's 28 px left margin
and ended the box 44 px short of Cancel: the label from the padding, the box to the right padding, OK and Cancel 24 px tall on the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - s.PAD
    fx = s.col_x(h, ["label4"])
    s.row(h, "label4", "textPassword", s.PAD, fx, R - fx)
    s.bar(h, right=("butOK", "butCancel"), bh=24)
