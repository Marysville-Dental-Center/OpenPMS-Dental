"""FormConnectionLost on the grid (UI-12 slice 8, layout script on the planner's tree). panel1 is the window's frame (at 0, 0 over the
whole client area, anchored on every side, the error label and the buttons drawn over it); the planner moved it to 8, 8 and grew the
window from main's 400 x 200 to 416 x 252: the frame back at 0, 0 in main's size, the buttons on the bottom padding inside it (Exit
Program on the right padding, Retry 8 px before it), the error label from the padding to the right padding and down to 8 px over them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_W, MAIN_H = 400, 200


def layout(h):
    s.size(h, MAIN_W, MAIN_H)
    h.set("panel1", 0, 0, MAIN_W, MAIN_H)
    yb = s.bar(h, right=("butRetry", "butExit"))
    h.set("labelErrMsg", 8, 8, MAIN_W - 16, yb - 8 - 8)
