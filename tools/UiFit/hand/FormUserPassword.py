"""FormUserPassword on the grid (UI-12 slice 9, layout script on the planner's tree): the code names the password rows New Password and
Re-Enter Password when the user sets a first password (grid-runtime-texts/9.tsv): the label column as wide as the longest of the texts."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    lw = max(h.lw(n) for n in ("label3", "labelCurrent", "labelNew"))
    F = h.n("textUserName").nx
    for lab in ("label3", "labelCurrent", "labelNew"):
        h.set(lab, F - 6 - lw, None, lw, None)
        h.text_align(lab, "MiddleRight")
