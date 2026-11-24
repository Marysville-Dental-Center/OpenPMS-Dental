"""FormSshHostKeys on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the window 1100 -> 1108 wide; the note
and the grid stood 12 px from the left, Remove ended 25 px short of the right, a 61 px empty band stood under the key rows: main's size, on
the padding, Remove on the right padding with Accept Changed Key 6 px before it and the key boxes to 6 px before that, the three rows 12 px
over the bar, the grid 8 px over them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 1100, 556)
    R = s.right(h)
    s.bar(h, left=("butRefresh",), right_=("butClose",))
    yb = h.n("butClose").ny
    y3 = yb - 12 - 23
    for i, (t, l) in enumerate((("textRefusedKey", "labelRefusedKey"), ("textStoredKey", "labelStoredKey"), ("textServer", "labelServer"))):
        h.set(t, None, y3 - i * 27, None, 23)
        s.label_before(h, l, t)
    s.row_right(h, ("butAccept", "butRemove"), R, y3)
    r = h.n("butAccept").nx - 6
    for t in ("textRefusedKey", "textStoredKey"):
        s.span(h, t, r=r)
    h.set("labelIntro", 8, None, R - 8, None)
    s.fill(h, "gridMain", y=42, b=h.n("textServer").ny - 8)
