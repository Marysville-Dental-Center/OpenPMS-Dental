"""FormLetters on the grid (UI-12 slice 7, layout script on the planner's tree). The planner moved the letter text box up with its height
kept (83 px of empty window under it), left the column at 19 px and the note 836 px wide (the window 5 px wider than main's 858): main's
858 px window, the note, the caption, the list and its buttons on the content edge (Edit ending on the list's edge), the text box 8 px
after the list to the right edge and down to 8 px over the bottom."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W = 858
    h.set(".", None, None, W, None)
    R = W - 8
    s.span(h, "label2", x=8, r=R)
    h.set("label2", None, None, None, h.note_h("label2", R - 8))
    for n in ("label1", "butAdd", "butDelete"):
        s.move(h, n, x=8)
    lst = h.n("listLetters")
    s.move(h, "listLetters", x=8)
    e = h.n("butEdit")
    s.move(h, "butEdit", x=8 + lst.nw - e.nw)
    s.span(h, "textBody", x=8 + lst.nw + 8, r=R, b=h.tree.nh - 8)
