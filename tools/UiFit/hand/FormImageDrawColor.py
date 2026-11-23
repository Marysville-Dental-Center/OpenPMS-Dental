"""FormImageDrawColor on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 146 to
153 px, kept the note at x 16 and drew Transparent 40 px tall for its one line: main's height; the note on the padding, the two color
rows on the pitch 4 px under it, Transparent one line tall centred on its row, Save on the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_H = 146


def layout(h):
    s.size(h, None, MAIN_H)
    lm = h.n("labelMount")
    nw = s.W(h) - 16
    nh = h.note_h("labelMount", nw)
    h.set("labelMount", 8, 8, nw, nh)
    y1 = 8 + nh + 4
    y2 = y1 + 27
    for name in ("labelColorFore", "butColorFore"):
        h.set(name, None, y1)
    for name in ("labelColorTextBack", "butColorTextBack"):
        h.set(name, None, y2)
    h.set("checkTransparent", None, y2 + 2, None, 20)
    s.bar(h, right=("butSave",))
