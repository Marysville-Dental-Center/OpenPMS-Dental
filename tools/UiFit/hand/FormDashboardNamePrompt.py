"""FormDashboardNamePrompt on the grid (UI-12 slice 12, layout script on the planner's tree). The label from the padding, the box to the
right padding (it ended 13 px short of Cancel); OK and Cancel 24 px tall on the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 387, 8 + 23 + s.SECTION + 24 + 8)  # the section gap over the bar (main 71, the planner 74)
    R = s.W(h) - s.PAD
    fx = s.col_x(h, ["label1"])
    s.row(h, "label1", "textBoxTabName", s.PAD, fx, R - fx)
    s.bar(h, right=("butOk", "butCancel"), bh=24)
