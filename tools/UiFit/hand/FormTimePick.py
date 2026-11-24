"""FormTimePick on the grid (UI-12 slice 9, layout script on the planner's tree): the planner put AM after the minute box and PM under the
minute box, 62 px apart: the hour box, the colon and the minute box on one row, AM and PM a list 8 px after the minute box (AM on the row,
PM 4 px under it), the group as tall as the list, OK 12 px under the group.
Fix round: the date picker (200 px) left 7 px on the left and 28 on the right of Pick Date: it ends on the group's content edge (8 px
from its right edge); Pick Time keeps its compact row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    gd = h.n("groupDate")
    s.span(h, "dateTimePicker", x=8, r=gd.nw - 8)
    g = h.n("groupTime")
    top = h.n("comboHour").ny
    s.move(h, "comboHour", x=8, y=top)
    hx = 8 + h.n("comboHour").nw
    h.set("label1", hx + 2, top, 12, h.H)
    h.text_align("label1", "MiddleCenter")
    mx = hx + 2 + 12 + 2
    s.move(h, "comboMinute", x=mx, y=top)
    rx = mx + h.n("comboMinute").nw + 8
    ch = h.CH
    ya = top + (h.H - ch) // 2
    for i, r in enumerate(("radioAM", "radioPM")):
        h.check_left(r)
        h.set(r, rx, ya + i * (ch + 4), h.cw(r), ch)
    gh = ya + 2 * ch + 4 + 8
    h.set("groupTime", None, None, None, gh)
    ok = h.n("butOK")
    s.move(h, "butOK", y=g.ny + gh + 12)
    h.set(".", None, None, h.n(".").nw, g.ny + gh + 12 + ok.nh + 8)
