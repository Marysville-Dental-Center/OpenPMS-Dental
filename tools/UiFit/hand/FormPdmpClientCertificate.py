"""FormPdmpClientCertificate on the grid (UI-12 slice 9, layout script on the planner's tree): the intro note was a line taller than its two
lines (a 14 px band before the clinic line, 2 px after it): the note as tall as its lines, then the clinic line, the caption and its box,
the import group and the store buttons, 4 px apart within a block and 8 px between blocks.
Fix round: the compacted rows left 40 px of the window's height over Close: the Current certificate box takes the slack, so the store
buttons stand 12 px over Close."""
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
    ytc = y
    gi, bs, cl = h.n("groupImport"), h.n("butStore"), h.n("butClose")
    tch = cl.ny - 12 - bs.nh - 8 - gi.nh - 8 - ytc
    h.set("textCurrent", 8, ytc, R - 8, tch)
    y += tch + 8
    h.set("groupImport", 8, y, R - 8, None)
    y += h.n("groupImport").nh + 8
    s.move(h, "butStore", x=8, y=y)
    s.move(h, "butRemove", x=8 + h.n("butStore").nw + 6, y=y)
