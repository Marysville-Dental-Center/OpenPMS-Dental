"""FormSheetOutputFormat on the grid (UI-12 slice 6, layout script on the planner's tree). The paper copies box stood at 30 and the check boxes
and their boxes at 78, the check boxes two lines tall for a one-line text: the copies box at the padding with its label 6 px after it; each
check box at the padding on one line, its e-mail box under its text to the right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    h.set("textPaperCopies", 8, 8)
    h.set("label1", 8 + 44 + 6, 10)
    y = 35
    for chk, txt in (("checkEmailPat", "textEmailPat"), ("checkEmail2", "textEmail2")):
        h.check_left(chk)
        h.set(chk, 8, y, R - 8, 20)
        h.set(txt, 24, y + 20 + 4, R - 24, None)
        y += 20 + 4 + 23 + 8
