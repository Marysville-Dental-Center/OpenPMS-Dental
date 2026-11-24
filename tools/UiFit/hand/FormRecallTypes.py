"""FormRecallTypes on the grid (UI-12 slice 10, layout script on the planner's tree). The planner put the Prophy/Perio check box over Add and
Sync (no fixed point): main's rows, Add and Sync with the resynchronization note 6 px after Sync, the check box 8 px under them on the
bottom padding; the grid on the padding, 12 px over the button row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    B = s.bottom(h)
    c = h.n("checkAutoSwitchProphyPerio")
    h.set("checkAutoSwitchProphyPerio", 8, B - c.nh, None, None)
    y = B - c.nh - 8 - 24
    x = s.row_left(h, ("butAdd", "butSynch"), 8, y)
    h.set("label1", x + 6, y, None, 24)
    h.text_align("label1", "MiddleLeft")
    s.fill(h, "gridMain", y=8, b=y - 12)
