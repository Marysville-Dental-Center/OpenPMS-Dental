"""FormCentralPatientSearch on the grid (UI-12 slice 12, layout script on the planner's tree). The planner put OK over Cancel (main stacked
them in the search column) and left the column 44 px under the grid's top: the grid from the padding to the bottom padding; the search
column on the right padding at the grid's top: the Search by group as the planner laid it out, the Limit check box under it, Search on the
column's right edge with the fetching note (hidden in the designer) right-aligned before it; OK and Cancel on the bottom right under the
column. The window keeps the planner's 743 x 667 (the column's 14 search rows do not fit main's 566)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - s.PAD
    B = s.H(h) - s.PAD
    g2 = h.n("groupBox2")
    cx = R - g2.nw
    h.set("gridPats", s.PAD, s.PAD, cx - s.GAP - s.PAD, B - s.PAD)
    h.set("groupBox2", cx, s.PAD)
    y = s.PAD + g2.nh + s.GAP
    h.check_left("checkLimit")
    h.set("checkLimit", cx + s.PAD, y, h.cw("checkLimit"), h.CH)
    y += h.CH + s.ROWGAP
    b = h.n("butSearch")
    h.set("butSearch", R - b.nw, y, None, h.H)
    lf = h.n("labelFetch")
    h.set("labelFetch", R - b.nw - s.LGAP - lf.nw, y, None, h.H)
    h.text_align("labelFetch", "MiddleRight")
    s.bar(h, right=("buttonOK", "butClose"), bh=24)
