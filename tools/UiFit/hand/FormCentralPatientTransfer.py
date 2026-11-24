"""FormCentralPatientTransfer on the grid (UI-12 slice 12, layout script on the planner's tree). The two grids at two tops (22 and 32) and
three left margins: the source caption (its text set by the code) 2 px over the patients grid; the two grids side by side at one top and
one bottom, 8 px apart, down to the section gap over the bar; Export in the column beside them on their bottom edge; each grid's Add and
Remove on the bar under it from its left edge, Close on the bottom right."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - s.PAD
    bw = 75
    cx = R - bw
    gw = (cx - s.GAP - s.PAD - s.GAP) // 2
    yb = s.bar(h, left=("butAddPatients", "butRemovePats"), right=("butClose",), bh=24)
    h.set("labelSourceDb", s.PAD, s.PAD, gw, 17)
    h.text_align("labelSourceDb", "BottomLeft")
    top = s.PAD + 17 + s.CAPGAP
    bottom = yb - s.SECTION
    h.set("gridPatients", s.PAD, top, gw, bottom - top)
    x2 = s.PAD + gw + s.GAP
    h.set("gridDatabasesTo", x2, top, gw, bottom - top)
    h.set("butAddDatabases", x2, yb, bw, 24)
    h.set("butRemoveDb", x2 + bw + s.GAP, yb, bw, 24)
    h.set("butTransfer", cx, bottom - 24, bw, 24)
