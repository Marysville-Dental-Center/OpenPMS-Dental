"""FormQBAccountSelect on the grid (UI-12 slice 9, layout script on the planner's tree): the Income Account caption was 11 px tall and cut
by the box above it (main too), the content at 12 against OK's 8: the note, then each caption 2 px over its box, 8 px between the pairs,
from the padding; OK on the bottom bar 12 px under the last box (the window 13 px taller)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W = h.n(".").nw
    y = 8
    h.set("label1", 8, y, W - 16, 16)
    y += 16 + 8
    for cap, box in (("labelDepositAccount", "comboDepositAccount"), ("labelIncomeAccountQB", "comboIncomeAccount")):
        h.set(cap, 8, y, h.n(box).nw, 16)
        y += 16 + 2
        h.set(box, 8, y, None, None)
        y += h.n(box).nh + 8
    y += 4
    h.set(".", None, None, W, y + h.n("butOK").nh + 8)
    s.move(h, "butOK", x=W - 8 - h.n("butOK").nw, y=y)
