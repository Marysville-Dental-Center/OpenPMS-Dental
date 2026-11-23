"""FormNotePick on the grid (UI-12 slice 7, layout script on the planner's tree). The notes, the grid and the note box stood 29 px from the
left and 28 px short of OK's edge: on the content edges (the notes' texts change for a Trojan import: grid-runtime-texts)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    for n in ("label1", "gridMain", "label2", "textNote"):
        s.span(h, n, x=8, r=R)
