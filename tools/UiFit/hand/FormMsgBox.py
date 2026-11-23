"""FormMsgBox on the grid (UI-12 slice 9, layout script on the planner's tree): the planner stacked OK over Cancel and grew the window 54
px. Main's arrangement on the grid: the message from the padding to the bottom bar, OK and Cancel on the bar at the bottom right, 8 px
apart, the window at main's 284 x 104 (the code moves OK onto Cancel's place when it hides Cancel)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W, HT = 284, 104
    h.set(".", None, None, W, HT)
    y = HT - 8 - h.n("butOK").nh
    s.row_right(h, ("butOK", "butCancel"), W - 8, y=y, gap=8)
    h.set("labelText", 8, 8, W - 16, y - 8 - 8)
