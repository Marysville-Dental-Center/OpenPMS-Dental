"""The report designer's ReportCtl on the grid (UI-12 slice 13, layout script on the planner's tree; its designer code is in ReportCtl.cs,
read and edited in InitializeComponent's body). PropertyDialog docks it in a 450 x 300 tab page: the control keeps main's 472 x 288 (the
planner made it 556 x 311, Page Footer past the page). The labels from the padding (main's x 16); Author and Description to the right
edge; Page and Margins in a left column, their left boxes on the window's field column (64 px, like
Width), Page Header and Page Footer beside them with their check boxes as a
list, the i-th groups of the two columns with one top and one height."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def lab(h, name, x, y, w):
    h.text_align(name, "MiddleRight")
    h.set(name, x, y, w, h.H)


def layout(h):
    R = 448
    h.set(".", None, None, 472, 288)
    lw = max(h.lw(n) for n in ("label3", "label2", "label9"))
    fx = 8 + lw + 6
    for i, (l, t) in enumerate((("label3", "tbReportAuthor"), ("label2", "tbReportDescription"), ("label9", "tbWidth"))):
        lab(h, l, 8, 8 + 27 * i, lw)
        h.set(t, fx, 8 + 27 * i, (R - fx) if i < 2 else 64, h.H)
    # the groups' left boxes on the window's field column (the group's content is 8 px further in), 64 px like Width
    gl = fx - 8 - 6 - 8
    gr = max(h.lw(n) for n in ("label4", "label6", "label7"))
    rx = 8 + gl + 6 + 64 + 16
    for l1, t1, l2, t2, y in (("label1", "tbPageWidth", "label4", "tbPageHeight", 23), ("label5", "tbMarginLeft", "label6", "tbMarginRight", 23),
                              ("label8", "tbMarginTop", "label7", "tbMarginBottom", 50)):
        lab(h, l1, 8, y, gl)
        h.set(t1, 8 + gl + 6, y, 64, h.H)
        lab(h, l2, rx, y, gr)
        h.set(t2, rx + gr + 6, y, 64, h.H)
    lgw = rx + gr + 6 + 64 + 8
    cx = 8 + lgw + 8
    cw = R - cx
    for g in ("groupBox3", "groupBox4"):
        y = 23
        for c in (("chkPHFirst", "chkPHLast") if g == "groupBox3" else ("chkPFFirst", "chkPFLast")):
            h.check_left(c)
            h.set(c, 8, y, cw - 16, h.CH)
            y += h.CH + 4
    h1 = max(23 + h.H + 8, 23 + 2 * h.CH + 4 + 8)
    h2 = max(23 + 27 + h.H + 8, 23 + 2 * h.CH + 4 + 8)
    y1 = 8 + 2 * 27 + h.H + 8
    h.set("groupBox1", 8, y1, lgw, h1)
    h.set("groupBox3", cx, y1, cw, h1)
    h.set("groupBox2", 8, y1 + h1 + 8, lgw, h2)
    h.set("groupBox4", cx, y1 + h1 + 8, cw, h2)
