"""The report designer's DialogNewChart on the grid (UI-12 slice 13, layout script on the planner's tree; its designer code is in
DialogNewChart.cs, read and edited in InitializeComponent's body). The planner widened the window 14 px, left the Chart Series caption
right-aligned beside its Up button, the Chart Cell Expression caption over its box (overlap), the lists at two left edges and the Delete
buttons wider than Up and Down. Main's 456 x 424: DataSet, Chart Type and Sub-type as label + box rows; the captions over the lists; the
fields list down to the series list's bottom, the categories and series lists one under the other with one height, each with its arrow
before it and its Up, Down and Delete (56 px) after it; Chart Cell Expression's caption over its box over the width; OK and Cancel on the bar
under the section gap."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def cap(h, name, x, y, w):
    h.text_align(name, "BottomLeft")
    h.set(name, x, y, w, 17)
    return y + 17 + 2


def layout(h):
    W0, H0 = 456, 424
    R = W0 - 8
    h.set(".", None, None, W0, H0)
    top = s.bar(h, right=("bOK", "bCancel"))
    lw = max(h.lw("label1"), h.lw("label7"))
    fx = 8 + lw + 6
    for lab, y in (("label1", 8), ("label7", 35)):
        h.text_align(lab, "MiddleRight")
        h.set(lab, 8, y, lw, h.H)
    h.set("cbDataSets", fx, 8, R - fx, h.H)
    h.set("cbChartType", fx, 35, 120, h.H)
    sw = h.lw("label6")
    sx = fx + 120 + 16
    h.text_align("label6", "MiddleRight")
    h.set("label6", sx, 35, sw, h.H)
    h.set("cbSubType", sx + sw + 6, 35, 80, h.H)
    LW = 152
    xl, xa, xr = 8, 8 + LW + 8, 8 + LW + 8 + 32 + 8
    xb = xr + LW + 8
    y = 35 + h.H + 8
    cap(h, "label2", xl, y, LW)
    ly = cap(h, "label3", xr, y, R - xr)
    ey = top - s.SECTION - h.H  # Chart Cell Expression's box
    cy = ey - 2 - 17
    bottom = cy - 8
    lh = (bottom - ly - 8 - 19) // 2
    h.set("lbFields", xl, ly, LW, bottom - ly)
    for lst, arrow, buts, yy in (("lbChartCategories", "bCategory", ("bCategoryUp", "bCategoryDown", "bCategoryDelete"), ly),
                                 ("lbChartSeries", "bSeries", ("bSeriesUp", "bSeriesDown", "bSeriesDelete"), bottom - lh)):
        h.set(lst, xr, yy, LW, lh)
        h.set(arrow, xa, yy, 32, 28)
        for i, b in enumerate(buts):
            h.set(b, xb, yy + 32 * i, 56, 28)
    cap(h, "label4", xr, bottom - lh - 19, LW)
    cap(h, "label5", 8, cy, R - 8)
    h.set("cbChartData", 8, ey, R - 8, h.H)
