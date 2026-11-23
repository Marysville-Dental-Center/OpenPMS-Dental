"""FormGuardianEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 372 to 444
px to put the Guardian note 6 px after the check box (main drew it over the field column's right edge, past its 372 px): the window as
wide as that row needs (the note's text on one line) and no wider."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    l4 = h.n("label4")
    w = h.lw("label4")
    h.set("label4", None, None, w, None)
    s.size(h, l4.nx + w + 8, None)
    s.bar(h, left=("butDelete",), right=("butSave",))
