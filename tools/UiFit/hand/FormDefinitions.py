"""FormDefinitions from main's arrangement on the grid (UI-12 slice 7, layout script on the planner's tree). The planner put 55 px between the
Guidelines caption and its box, moved the grid 3 px above the category list and grew the window from 707 to 762 px: main's 707 px window,
the caption and the category list on the content edge (the list down to 8 px over the bottom), the definitions grid 8 px after the list on
the list's top, the Edit Items group 8 px under the grid (its five buttons 79 px, 6 apart), the Guidelines caption 2 px over its box, the
box on the bottom edge; the grid, the group and the box end on the right edge. Fix round (the lead's review of 2026-11-23): the five Edit
Items buttons across the group's content (they were packed at its left, 71 px empty on the right): 90 px each, 8 px apart, 8 px from both
sides."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    H = 707
    h.set(".", None, None, None, H)
    R = s.right(h)
    B = H - 8
    h.set("label13", 8, 8, None, 17)
    h.text_align("label13", "BottomLeft")
    lst = h.n("listCategory")
    s.span(h, "listCategory", x=8, r=8 + lst.nw, y=27, b=B)
    X = 8 + lst.nw + 8
    s.span(h, "textGuide", x=X, r=R, y=B - 63, b=B)
    h.set("label14", X, B - 63 - 2 - 17, None, 17)
    h.text_align("label14", "BottomLeft")
    gy = B - 63 - 2 - 17 - 8 - 54
    s.span(h, "groupEdit", x=X, r=R, y=gy, b=gy + 54)
    bw = (h.n("groupEdit").nw - 16 - 4 * 8) // 5
    for n in ("butAdd", "butHide", "butUp", "butDown", "butAlphabetize"):
        h.set(n, None, None, bw, None)
    s.row_left(h, ("butAdd", "butHide", "butUp", "butDown", "butAlphabetize"), 8, y=22, gap=8)
    s.span(h, "gridDefs", x=X, r=R, y=27, b=gy - 8)
