"""The report designer's ChartAxisCtl on the grid (UI-12 slice 13, layout script on the planner's tree; its designer code is in
ChartAxisCtl.cs, read and edited in InitializeComponent's body). PropertyDialog docks it in a 450 px tab page: the control keeps main's
440 px (the planner made it 525 for Interlaced, left alone at x 440). The planner left the grid line groups' Color and Width labels over
the boxes after them. The two bands of label + box rows (Tick Marks; Interval and Value with their fx buttons) on two label columns; in
each grid line group one row from its content edge, each label 6 px after the control before it and its box 6 px after it (the boxes 64
px, Width 40); the six check boxes on a grid of two rows and three columns from the field column (Interlaced with them)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def band_row(h, label, box, x, lw, y, w, fx=None):
    h.text_align(label, "MiddleRight")
    h.set(label, x, y, lw, h.H)
    h.set(box, x + lw + 6, y, w, h.H)
    if fx:
        h.set(fx, x + lw + 6 + w + 6, y, 22, h.H)
    return x + lw + 6 + w + (6 + 22 if fx else 0)


def layout(h):
    h.set(".", None, None, 440, None)
    lw = max(h.lw(n) for n in ("label1", "label9", "label12"))
    rw = max(h.lw(n) for n in ("label2", "label10", "label11"))
    end = band_row(h, "label1", "cbMajorTickMarks", 8, lw, 8, 80)
    rx = 8 + lw + 6 + 64 + 6 + 22 + 16
    band_row(h, "label2", "cbMinorTickMarks", rx, rw, 8, 80)
    for i, (g, chk, ls, cs, lc, cc, bc, lwid, tw) in enumerate((
            ("groupBox1", "chkMajorGLShow", "label3", "cbMajorGLStyle", "label7", "cbMajorGLColor", "bMajorGLColor", "label6", "tbMajorGLWidth"),
            ("groupBox2", "chkMinorGLShow", "label8", "cbMinorGLStyle", "label4", "cbMinorGLColor", "bMinorGLColor", "label5", "tbMinorGLWidth"))):
        h.check_left(chk)
        h.set(chk, 8, 23 + (h.H - h.CH) // 2, 16 + h.n(chk).text_w + 2, h.CH)
        x = 8 + h.n(chk).nw + 6
        for lab, box, w in ((ls, cs, 64), (lc, cc, 64)):
            lw2 = h.lw(lab)
            h.text_align(lab, "MiddleRight")
            h.set(lab, x, 23, lw2, h.H)
            h.set(box, x + lw2 + 6, 23, w, h.H)
            x += lw2 + 6 + w + 6
        h.set(bc, x, 23, 24, h.H)
        x += 24 + 6
        lw2 = h.lw(lwid)
        h.text_align(lwid, "MiddleRight")
        h.set(lwid, x, 23, lw2, h.H)
        h.set(tw, x + lw2 + 6, 23, 40, h.H)
        h.set(g, 8, 39 + 63 * i, 424, 23 + h.H + 8 + 1)
    y = 39 + 2 * 63 + 8
    band_row(h, "label9", "tbMajorInterval", 8, lw, y, 64, "bMajorIntervalExpr")
    band_row(h, "label10", "tbMinorInterval", rx, rw, y, 64, "bMinorIntervalExpr")
    band_row(h, "label12", "tbMin", 8, lw, y + 27, 64, "bMinExpr")
    band_row(h, "label11", "tbMax", rx, rw, y + 27, 64, "bMaxExpr")
    y += 27 + h.H + 8
    grid = (("chkVisible", "chkLogScale", "chkMargin"), ("chkScalar", "chkReverse", "chkInterlaced"))
    widths = [max(16 + h.n(r[c]).text_w + 4 for r in grid) for c in range(3)]
    # Fix round (review of slice 13): the columns on the bands' columns (the second and third stood on nothing above): the first on the left
    # field column, the third on the right band's field column, the second on the left fx button column (or centred between the first and
    # the third when the first does not end 8 px before it).
    x1 = 8 + lw + 6
    x3 = rx + rw + 6
    fx_col = x1 + 64 + 6
    x2 = fx_col if x1 + widths[0] + 8 <= fx_col else (x1 + widths[0] + x3 - widths[1]) // 2
    assert x2 + widths[1] + 8 <= x3 and x3 + widths[2] <= 440 - 8, "ChartAxisCtl: the check box columns do not fit"
    for r, row in enumerate(grid):
        for c, name in enumerate(row):
            h.check_left(name)
            h.set(name, (x1, x2, x3)[c], y + r * (h.CH + 4), 16 + h.n(name).text_w + 4, h.CH)
