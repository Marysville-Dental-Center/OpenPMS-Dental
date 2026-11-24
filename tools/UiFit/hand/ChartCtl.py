"""The report designer's ChartCtl on the grid (UI-12 slice 15, layout script on the planner's tree; designer code in ChartCtl.cs). The
planner kept the percent caption run into its box (overlap) and mixed left- and right-aligned labels. One label column (right-aligned,
Render XML Element the widest) before the field column in main's order: Chart Type with Sub-type after it on its row, Palette, Render
XML Element, the percent caption over its box from the padding (382 px: one line only over the whole width), No Rows Message, Data Set
Name, Chart Data, then the two page break check boxes on the field column; main's 440 x 288 (a property page)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    W0 = 440
    R = W0 - 8
    h.set(".", None, None, W0, 288)
    lw = max(h.lw(n) for n in ("label1", "label2", "label3", "label6", "label7", "label8"))
    fx = 8 + lw + s.ITEM
    s.row(h, "label1", "cbChartType", 8, lw, 120, 8)
    sw = h.lw("label5")
    sx = fx + 120 + s.BAND
    s.row(h, "label5", "cbSubType", sx, sw, R - (sx + sw + s.ITEM), 8)
    s.row(h, "label2", "cbPalette", 8, lw, 120, 35)
    s.row(h, "label3", "cbRenderElement", 8, lw, 120, 62)
    y = s.caption(h, "label4", 8, 89, R - 8)
    h.set("tbPercentWidth", 8, y, 64, h.H)
    y += 27
    s.row(h, "label6", "tbNoRows", 8, lw, R - fx, y)
    s.row(h, "label7", "cbDataSet", 8, lw, 120, y + 27)
    s.row(h, "label8", "cbChartData", 8, lw, R - fx, y + 54)
    x = s.chk(h, "chkPageBreakStart", fx, y + 81)
    s.chk(h, "chkPageBreakEnd", x + s.BAND, y + 81)
