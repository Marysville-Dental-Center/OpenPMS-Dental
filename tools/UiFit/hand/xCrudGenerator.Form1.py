"""xCrudGenerator's Form1 on the grid (UI-12 slice 12, layout script on the planner's tree; hand/<namespace>.<class>.py). The planner grew
the window 20 px and left the settings in three margins: back to main's 1161 x 734. The settings column from the padding: Port Number,
Database and Password rows, then Schema (no db needed) and Run with their text-less check boxes on the field column, the two convert
script check boxes as a list, Run CRUD Gen on the field column; the two notes beside the column (the band gap) up to the sibling gap before
Jordan's Pd on the right padding; under all of it the snippet group and the Snippets caption and box side by side down to the bottom
padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 1161, 734)
    R = 1161 - s.PAD
    B = 734 - s.PAD
    fx = s.col_x(h, ["labelPortNum", "label3", "label7", "label9", "label8"])
    s.row(h, "labelPortNum", "textPortNum", s.PAD, fx)
    s.row(h, "label3", "textDb", s.PAD + 27, fx)
    yb = s.row(h, "label7", "textPassword", s.PAD + 54, fx)
    y = yb + s.ROWGAP
    for lab, chk in (("label9", "checkRunSchema"), ("label8", "checkRun")):
        h.set(lab, s.PAD, y, fx - s.LGAP - s.PAD, h.H)
        h.text_align(lab, "MiddleRight")
        h.check_left(chk)
        h.set(chk, fx, y + (h.H - h.CH) // 2, 16, h.CH)
        y += h.H + s.ROWGAP
    for chk in ("checkAddToConvertScript", "checkRunQueries"):
        h.check_left(chk)
        h.text_align(chk, "MiddleLeft")
        h.set(chk, s.PAD, y, h.cw(chk), h.CH)
        y += h.CH + s.ROWGAP
    h.set("butRun", fx, y, None, h.H)
    left_bottom = y + h.H
    colr = max(fx + h.n("textDb").nw, s.PAD + h.cw("checkRunQueries"))
    nx = colr + 16
    h.set("butPd", R - 75, s.PAD, 75, h.H)
    nw = R - 75 - s.GAP - nx
    ny = s.note(h, "label1", nx, s.PAD, nw) + s.ROWGAP
    ny = s.note(h, "label10", nx, ny, nw)
    top = max(left_bottom, ny) + s.SECTION
    gw = 165
    cw = gw - 2 * s.PAD
    gy = s.TITLE
    h.set("butSnippet", s.PAD, gy, None, h.H)
    gy = s.note(h, "label6", s.PAD, gy + h.H + s.ROWGAP, cw) + s.ROWGAP
    gy = h.captioned("label5", "comboType", s.PAD, gy, cw) + s.ROWGAP
    gh = B - top
    h.captioned("label4", "listClass", s.PAD, gy, cw, h=gh - s.PAD - (gy + 17 + s.CAPGAP))
    h.set("groupBox1", s.PAD, top, gw, gh)
    sx = s.PAD + gw + s.GAP
    h.set("label2", sx, top, h.lw("label2"), 17)
    h.text_align("label2", "BottomLeft")
    st = top + 17 + s.CAPGAP
    h.set("textSnippet", sx, st, R - sx, B - st)
