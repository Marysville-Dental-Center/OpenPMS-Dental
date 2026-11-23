"""FormClaimCustomTrackingUpdate on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from
main's 379 to 389 px and left the note box 9 px short of the right padding and the error description 1 px wider than the combos: main's
height, Save on the bottom padding, the note box from the padding to the right padding and down to 8 px over Save, the description as
wide as the combos."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_H = 379


def layout(h):
    s.size(h, None, MAIN_H)
    yb = s.bar(h, right=("butSave",))
    tn = h.n("textNotes")
    h.set("textNotes", 8, None, s.W(h) - 16, yb - 8 - tn.ny)
    h.set("textErrorDesc", None, None, h.n("comboErrorCode").nw, None)
