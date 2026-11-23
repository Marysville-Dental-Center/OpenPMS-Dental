"""FormDefaultCCProcs on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 473 to
496 px and kept the note at x 12, the list at 15 and the button column 5 px short of Save's edge: main's height; the note and the list
from the padding; Add, Remove and Sync Procs on the right padding from the list's top, 4 px apart; the list to 8 px before them and
down to 8 px over Save."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_H = 473


def layout(h):
    s.size(h, None, MAIN_H)
    R = s.W(h) - 8
    yb = s.bar(h, right=("butSave",))
    nw = R - 8
    nh = h.note_h("label15", nw)
    h.set("label15", 8, 8, nw, nh)
    top = 8 + nh + 2
    bw = h.n("butAddProc").nw
    y = top
    for name in ("butAddProc", "butRemoveProc", "butSync"):
        h.set(name, R - bw, y)
        y += h.n(name).nh + 4
    h.set("listProcs", 8, top, R - bw - 8 - 8, yb - 8 - top)
