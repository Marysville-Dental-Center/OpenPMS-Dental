"""FormCodeGroupEdit (Code Group) on the grid (UI-12 slice 1, layout script of grid_hand.py). The planner cut the two rows of three right-aligned
check boxes as rows of label + field pairs and widened the window to 1081 px; the script lays out the window as drawn: the six check boxes in
three columns on the field column (the box first), the four label + field rows under them on one field column, the Fixed Group note beside its
row, the Proc Codes note under its box, the bottom bar. The window keeps its 671 px width."""


def layout(h):
    H = h.H
    p = h.box(".")
    labels = ("label1", "label3", "label2", "labelHistProcCode")
    lw = max(h.lw(x) for x in labels)
    F = p.left + lw + 6
    # The check boxes: three columns from the field column, one column per pair, 16 px between a column's widest text and the next column.
    cols = [("checkShowInAgeLim", "checkShowInFreq"), ("checkShowInOther", "checkHidden"), ("checkShowInHistory", "checkPerioFourQuads")]
    xs = []
    x = F
    for col in cols:
        xs.append(x)
        x += max(h.cw(n) for n in col) + 16
    p.row(*[(col[0], xs[i], None) for i, col in enumerate(cols)], gap=4)
    p.row(*[(col[1], xs[i], None) for i, col in enumerate(cols)])
    p.y += 4
    for lb in labels:
        h.text_align(lb, "MiddleRight")
    p.field("label1", "textGroupName", F, 168, lw=lw)
    yf = p.y
    p.field("label3", "comboCodeGroupFixed", F, 168, lw=lw)
    # The note beside Fixed Group: from 6 px after the combo box to the content edge, on the row's top, as tall as its four lines.
    xn = F + 168 + 6
    h.set("label5", xn, yf, p.right - xn, 68)
    p.y = max(p.y, yf + 68 + 4)
    yp = p.y
    wp = p.right - F - 6 - 80
    h.set("label2", F - 6 - lw, yp, lw, H)
    h.set("textProcCodes", F, yp, wp, 49)
    h.set("butProcCodesAdd", F + wp + 6, yp, 80, H)
    p.y = yp + 49 + 2
    p.bottom_used = p.y
    h.set("label4", F, p.y, wp, 18)
    p.y += 18 + 4
    p.field("labelHistProcCode", "textHistProcCode", F, 100, after=[("butHistProcCode", None, 24)], lw=lw)
    # The bottom bar 8 px from the window's bottom; the window keeps its height (it never shrinks) unless the content needs more.
    hw = max(p.node.h, p.bottom_used + 12 + 24 + p.pad)
    yb = hw - p.pad - 24
    h.set("butDelete", p.left, yb, 75, 24)
    h.set("butSave", p.node.nw - p.pad - 75, yb, 75, 24)
    p.end(h=hw)
