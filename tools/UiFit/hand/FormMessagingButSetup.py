"""FormMessagingButSetup on the grid (UI-12 slice 8, layout script on the planner's tree). The two lists stood at x 34 and 274 (40 px
apart, 102 px empty on the right) with a 59 px band between them and Up and Down: the lists one width from the padding to the right
padding 8 px apart, their captions 2 px over them; Up and Down under the Buttons list on its right edge, on the bottom padding; the lists
down to 8 px over them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    w = (R - 8 - 8) // 2
    yb = s.bar(h, right=("butUp", "butDown"))
    lc = h.n("listComputers")
    top = lc.ny
    for cap, lst, x in (("label2", "listComputers", 8), ("label1", "listButtons", 8 + w + 8)):
        h.set(cap, x, None)
        h.set(lst, x, top, (R - x) if x > 8 else w, yb - 8 - top)
