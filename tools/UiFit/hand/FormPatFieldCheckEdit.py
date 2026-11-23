"""FormPatFieldCheckEdit on the grid (UI-12 slice 8 fix round, ui/ui12-8-2; layout script on the planner's tree). The field name (a label
beside a check box with no text) stood at x 30, 22 px after the box's left edge: at the check box's text lead (the box's x + 16, UI-12
section 1.1, less the label's 2 px inner margin), centred on the box's row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    c = h.n("checkFieldValue")
    ln = h.n("labelName")
    x = c.nx + 16 - 2
    h.text_align("labelName", "MiddleLeft")
    h.set("labelName", x, c.ny + (c.nh - ln.nh) // 2, ln.nx + ln.nw - x, None)
