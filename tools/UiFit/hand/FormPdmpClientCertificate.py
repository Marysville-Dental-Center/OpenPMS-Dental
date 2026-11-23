"""FormPdmpClientCertificate on the grid (UI-12 slice 9, layout script on the planner's tree): the intro note was a line taller than its two
lines (a 14 px band before the clinic line, 2 px after it): the note as tall as its lines, then the clinic line, the caption and its box,
the import group and the store buttons, 4 px apart within a block and 8 px between blocks."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = h.n(".").nw - 8
    y = 8
    nh = h.note_h("labelIntro", R - 8)
    h.set("labelIntro", 8, y, R - 8, nh)
    y += nh + 4
    h.set("labelClinic", 8, y, R - 8, 18)
    y += 18 + 4
    h.set("labelCurrent", 8, y, None, 18)
    y += 18 + 2
    h.set("textCurrent", 8, y, R - 8, None)
    y += h.n("textCurrent").nh + 8
    h.set("groupImport", 8, y, R - 8, None)
    y += h.n("groupImport").nh + 8
    s.move(h, "butStore", x=8, y=y)
    s.move(h, "butRemove", x=8 + h.n("butStore").nw + 6, y=y)
