"""FormCentralUserSettings on the grid (UI-12 slice 12, layout script on the planner's tree). The User row with its box to the right
padding (it ended 31 px short of Cancel); the check box on the field column under it and, under the check box, the note the code shows
when automatic log in is off (hidden in the designer), as wide as the field column and as tall as its lines; OK and Cancel on the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 294, 145)  # main's size (the planner grew it 3 px)
    R = s.W(h) - s.PAD
    fx = s.col_x(h, ["label3"])
    yb = s.row(h, "label3", "textUserName", s.PAD, fx, R - fx)
    h.check_left("checkAutoLogin")
    h.set("checkAutoLogin", fx, yb + s.ROWGAP, h.cw("checkAutoLogin"), h.CH)
    s.note(h, "labelAutoLogin", fx, yb + s.ROWGAP + h.CH + s.ROWGAP, R - fx)
    s.bar(h, right=("butOK", "butCancel"), bh=24)
