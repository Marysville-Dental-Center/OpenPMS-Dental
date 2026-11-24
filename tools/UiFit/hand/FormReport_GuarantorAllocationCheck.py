"""FormReport_GuarantorAllocationCheck on the grid (UI-12 slice 9, layout script on the planner's tree): the planner set the report grid 2
px under the title band and left 96 px of empty window under the summary grid: the panel at the padding to the bottom; in it the title band,
the report grid 8 px under it, the Summary caption 8 px under that and the summary grid 2 px under the caption to the panel's padding (the
two grids share the room as on main, 411 to 316); the right column 8 px after the panel to the right content edge (191 px): the Guarantor
caption, its box, Fill Report and Print Preview, the From and To rows, 4 px apart. The window does not scale its positions (no
LayoutManager), so the column is as wide as the room allows: at 150% main cut Print Preview, From and the dates; the caption 24 px tall for
the same reason.
Fix round: at 150% the fonts grow and the positions do not, so Fill Report's descender and Print Preview's top were cut: the two
buttons 30 px tall (room for the 150% font), the rows 4 px apart as before."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    CW, CH = 1071, 973
    PW = 856
    ph = CH - 16
    h.set("panel1", 8, 8, PW, ph)
    t = h.n("lblTitle")
    h.set("lblTitle", 8, 8, PW - 16, t.nh)
    y1 = 8 + t.nh + 8
    room = ph - 8 - y1 - 8 - 21 - 2
    g1 = room * 411 // (411 + 316)
    h.set("dgridView_ReportData", 8, y1, PW - 16, g1)
    ys = y1 + g1 + 8
    h.set("lblSummary", 8, ys, PW - 16, 21)
    h.set("dgvSummary", 8, ys + 23, PW - 16, ph - 8 - (ys + 23))
    X = 8 + PW + 8
    CW2 = CW - 8 - X
    h.set("label2", X, 8, CW2, 24)
    y = 8 + 24 + 2
    for name in ("textBox1", "butFillReport", "butPrintPreview"):
        h.set(name, X, y, CW2, 30 if name.startswith("but") else None)
        y += h.n(name).nh + 4
    lw = 60
    for lab, box in (("label5", "dateTimePicker1_FromDate"), ("label6", "dateTimePicker1")):
        h.set(lab, X, y, lw, h.n(box).nh)
        h.text_align(lab, "MiddleRight")
        h.set(box, X + lw + 6, y, CW2 - lw - 6, None)
        y += h.n(box).nh + 4
