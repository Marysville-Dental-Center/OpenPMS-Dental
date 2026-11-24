"""FormDatabaseCheck on the grid (UI-12 slice 12, layout script on the planner's tree). Main's 87 px label margin and 29 px results margin:
the four rows with their labels from the padding and their boxes to the right padding, Check and Repair (24 px) under them ending on it,
the results box from the padding to the right padding down to the section gap over Copy Text, which stands on the bottom left."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - s.PAD
    fx = s.col_x(h, ["label2", "label1", "label3", "label4"])
    yb = s.rows(h, [("label2", "textComputerName"), ("label1", "textDatabase"), ("label3", "textUser"), ("label4", "textPassword")],
                s.PAD, fx, right=R)
    y = yb + s.ROWGAP
    h.set("butRepair", R - 75, y, 75, 24)
    h.set("butCheck", R - 75 - s.GAP - 75, y, 75, 24)
    top = y + 24 + s.GAP
    yc = s.bar(h, left=("butCopy",), bh=24)
    h.set("textResults", s.PAD, top, R - s.PAD, yc - s.SECTION - top)
