"""FormWarnToCloseComputers on the grid (UI-12 slice 9, layout script on the planner's tree): the message and the panel stood at 54 with the
question row 85 px over the bottom: the message and the panel from the padding to the right content edge, 8 px apart; the question with Yes
and No/Cancel on the bottom bar, the buttons 8 px from the edges and 8 px apart, the question right-aligned 6 px before Yes.
Fix round: the message and the panel ended at about 180 with an 85 px empty band over the question row: the message grows (its text
centred in its height) until the panel stands 12 px over the question row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    CW, CH = 783, 297
    y = CH - 8 - h.n("butNoCancel").nh
    p = h.n("panel1")
    py = y - 12 - p.nh
    h.set("lblMessage", 8, 8, CW - 16, py - 8 - 8)
    h.text_align("lblMessage", "MiddleLeft")
    h.set("panel1", 8, py, CW - 16, None)
    s.row_right(h, ("butYes", "butNoCancel"), CW - 8, y=y, gap=8)
    q = h.n("label1")
    h.set("label1", h.n("butYes").nx - 6 - q.nw, y, None, None)
    h.text_align("label1", "MiddleRight")
