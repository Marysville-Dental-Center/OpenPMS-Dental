"""GraphQuantityOverTime on the grid (UI-12 slice 12, layout script on the planner's tree). The settings over the chart sit in the first
panel of a split container, which the planner does not lay out: main's labels 15 px tall at four left edges, the rows 24 to 27 px apart,
the radio buttons at three x. The Chart Title row from the padding, its box ending on the last group's edge; the three groups under it at
the sibling gap, side by side 8 px apart at one top and one height: Chart Options and Filter Dates with their labels right-aligned 6 px
before one field column each and their rows at the pitch, Series Grouping with All, Top and None as a list from its content edge, the Top
count box and the series combo box after Top on its row; the split container's first panel as tall as the groups and the padding. The chart
and its legend (drawn by the code) stay as they are."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def settings(h, split, prefix=""):
    """Lays out the settings panel; returns the panel height it needs."""
    top = s.PAD + h.H + s.GAP
    # Chart Options.
    fx1 = s.col_x(h, ["label5", "label6", "label1", "label7"])
    # The Chart Title box on the Chart Options group's field column (the window's first field column).
    fxt = max(s.col_x(h, ["label8"]), s.PAD + fx1)
    yb1 = s.rows(h, [("label5", "comboQuantityType"), ("label6", "comboChartType"), ("label1", "comboGroupBy"), ("label7", "comboLegendDock")],
                 s.TITLE, fx1)
    w1 = fx1 + h.n("comboQuantityType").nw + s.PAD
    # Filter Dates.
    fx2 = s.col_x(h, ["label2", "label3", "label4"])
    yb2 = s.rows(h, [("label2", "comboQuickRange"), ("label3", "dateTimeFrom"), ("label4", "dateTimeTo")], s.TITLE, fx2)
    w2 = fx2 + h.n("comboQuickRange").nw + s.PAD
    # Series Grouping.
    y = s.TITLE
    h.check_left("radBreakdownAll")
    h.set("radBreakdownAll", s.PAD, y, h.cw("radBreakdownAll"), h.CH)
    y += h.CH + s.ROWGAP
    h.check_left("radBreakdownTop")
    h.set("radBreakdownTop", s.PAD, y + (h.H - h.CH) // 2, h.cw("radBreakdownTop"), h.CH)
    nx = s.PAD + max(h.cw("radBreakdownAll"), h.cw("radBreakdownTop"), h.cw("radBreakdownNone")) + s.LGAP
    h.set("numericTop", nx, y, None, h.H)
    cx = nx + h.n("numericTop").nw + s.LGAP
    h.set("comboBreakdownBy", cx, y, None, h.H)
    y += h.H + s.ROWGAP
    h.check_left("radBreakdownNone")
    h.set("radBreakdownNone", s.PAD, y, h.cw("radBreakdownNone"), h.CH)
    w3 = cx + h.n("comboBreakdownBy").nw + s.PAD
    gh = max(yb1, yb2, y + h.CH) + s.PAD
    x = s.PAD
    for name, w in (("groupBox1", w1), ("groupBox2", w2), ("groupBoxBreakdown", w3)):
        h.set(name, x, top, w, gh)
        x += w + s.GAP
    right = x - s.GAP
    s.row(h, "label8", "textChartTitle", s.PAD, fxt, right - fxt)
    need = top + gh + s.PAD
    h.splitter(split, need)
    return need


def layout(h):
    settings(h, "splitContainerMain")
