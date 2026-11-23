"""FormMountDefs on the grid (UI-12 slice 7, layout script on the planner's tree). The planner kept the note 385 px wide at x 8 (main's at 3)
and grew the window from 388 to 401 px for it; the list touched the note's last line and Up stood 9 px nearer Add than Down: main's 388 px
window, the note to the right edge as tall as its lines, the list 4 px under it, Add on the list's x, Down ending on the list's edge and
Up halfway between them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W = 388
    h.set(".", None, None, W, None)
    nh = h.note_h("label1", W - 16)
    h.set("label1", 8, 8, W - 16, nh)
    lst = h.n("listBoxMain")
    b = lst.ny + lst.nh
    s.span(h, "listBoxMain", x=8, y=8 + nh + 4, b=b)
    r = 8 + lst.nw
    a = h.n("butAdd")
    d = h.n("butDown")
    u = h.n("butUp")
    s.move(h, "butAdd", x=8)
    s.move(h, "butDown", x=r - d.nw)
    s.move(h, "butUp", x=(8 + a.nw + r - d.nw) // 2 - u.nw // 2)
