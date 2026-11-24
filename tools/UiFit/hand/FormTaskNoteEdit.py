"""FormTaskNoteEdit on the grid (UI-12 slice 9, layout script on the planner's tree): the note box ended 35 px short of Save's edge: it ends
on it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    sv = h.n("butSave")
    s.span(h, "textNote", r=sv.nx + sv.nw)
