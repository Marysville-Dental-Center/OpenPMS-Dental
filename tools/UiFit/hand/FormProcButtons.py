"""FormProcButtons on the grid (UI-12 slice 9, layout script on the planner's tree): the planner put the category list 24 px higher than the
buttons list (the stacked Quick Buttons panel and note made another row) and drew the note into the list. Main's two columns on the grid:
each caption over its list (the two lists one top and one height), Edit Categories under the left list, Add and Delete, Up and Down under
the right one; the Quick Buttons panel and its note, which the code shows instead of the list, on the list's place (the panel at its top,
the note 8 px under the panel); the lists down to the buttons under them, which end 8 px over the bottom (no empty band).
Fix round: Edit Categories (109 px) drew its plus icon 3 px before its text: 124 px wide, so the text stands clear of the icon."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W = h.n(".").nw
    LW = h.n("listCategories").nw
    RW = h.n("listViewButtons").nw
    x1 = 8
    x2 = W - 8 - RW
    h.set("label2", x1, 8, LW, 18)
    h.set("label1", x2, 8, RW, 18)
    top = 8 + 18 + 2
    HT = h.n(".").nh
    LH = HT - 8 - h.n("butAdd").nh - 4 - h.n("butDelete").nh - 8 - top
    h.set("listCategories", x1, top, None, LH)
    h.set("listViewButtons", x2, top, None, LH)
    h.set("panelQuickButtons", x2, top, None, None)
    pq = h.n("panelQuickButtons")
    h.set("labelEdit", x2, top + pq.nh + 8, RW, None)
    yb = top + LH + 8
    h.set("butEdit", x1, yb, 124, None)
    s.move(h, "butAdd", x=x2, y=yb)
    s.move(h, "butDelete", x=x2, y=yb + h.n("butAdd").nh + 4)
    xr = x2 + RW - h.n("butUp").nw
    s.move(h, "butUp", x=xr, y=yb)
    s.move(h, "butDown", x=xr, y=yb + h.n("butUp").nh + 4)
