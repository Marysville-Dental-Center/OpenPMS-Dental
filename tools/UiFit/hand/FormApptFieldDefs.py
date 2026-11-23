"""FormApptFieldDefs on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 383 to 396 px
and kept the note at x 15, the list at 18 and the button column 29 px short of the right padding: main's width, the note and the list
from the padding (8 px under the menu), the Add, Up and Down column on the right padding, the list to 8 px before it and down to the
bottom padding (the window has no bottom bar: it closes by its title bar)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_W = 383


def layout(h):
    s.size(h, MAIN_W, None)
    R = MAIN_W - 8
    nw = R - 8
    nh = h.note_h("label1", nw)
    h.set("label1", 8, 32, nw, nh)
    top = 32 + nh + 2
    bx = R - h.n("butAdd").nw
    y = top
    for name in ("butAdd", "butUp", "butDown"):
        h.set(name, bx, y)
        y += h.n(name).nh + 4
    h.set("listMain", 8, top, bx - 8 - 8, s.H(h) - 8 - top)
