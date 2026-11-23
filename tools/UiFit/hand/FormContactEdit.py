"""FormContactEdit on the grid (UI-12 slice 8, layout script on the planner's tree). Delete stood at x 42, the Notes box ended 45 px short
of the right padding with a 64 px empty band under it (the window keeps main's size): Delete on the padding, the Notes box to the right
padding and down to 8 px over the bottom bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    yb = s.bar(h, left=("butDelete",), right=("butSave",))
    tn = h.n("textNotes")
    h.set("textNotes", None, None, s.W(h) - 8 - tn.nx, yb - 8 - tn.ny)
