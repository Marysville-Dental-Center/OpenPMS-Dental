"""FormProcNoteAppend on the grid (UI-12 slice 10, layout script on the planner's tree). The two note boxes ended 4 px short of the right
padding (Save's edge): to it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    for n in ("textNotes", "textAppended"):
        s.span(h, n, r=R)
