"""FormEhrEduBrowser on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 884 x
639 by 14 x 15 to put its padding around the browser and left Print 83 px short of the right padding: main's size, the browser inside
the padding down to 8 px over Print, Print on the right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_W, MAIN_H = 884, 639


def layout(h):
    s.size(h, MAIN_W, MAIN_H)
    yb = s.bar(h, right=("butPrint",))
    h.set("webViewMain", 8, 8, MAIN_W - 16, yb - 8 - 8)
