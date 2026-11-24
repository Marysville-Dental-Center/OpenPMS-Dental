"""The report designer's DialogNewTable on the grid (UI-12 slice 13, layout script on the edit template's tree; its designer code is in
DialogNewTable.cs, read and edited in InitializeComponent's body). The template grew the window to 480 x 383 and left the radio buttons too
narrow for their text (the stock radio's circle and gap are 24 px), the lists at two left edges (8 and 16) and two heights, and the
captions at three heights. Main's 456 px width: DataSet over the width; Arrange Fields with Across and the two Down choices beside it as a list (one row
is 43 px too narrow); the
captions over the two lists (as tall as the two-line one, their text at the bottom), the lists on one top and height, the arrows between
them, Up and Down after the right list; the group column's caption and box under the left list, Calculate Grand Totals on the box's row
under the right list; OK and Cancel on the bar under the section gap."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


LEAD = 24  # a stock radio button's or check box's circle and gap before its text


def layout(h):
    W0 = 456
    R = W0 - 8
    lw = h.lw("label1")
    h.text_align("label1", "MiddleRight")
    h.set("label1", 8, 8, lw, h.H)
    h.set("cbDataSets", 8 + lw + 6, 8, R - (8 + lw + 6), h.H)
    # the three radio buttons do not fit one row (467 px for 424): Across, then the two Down choices as a list beside it
    w0 = LEAD + h.n("rbHorz").text_w
    h.check_left("rbHorz")
    h.set("rbHorz", 8, 23, w0, h.CH)
    for i, r in enumerate(("rbVert", "rbVertComp")):
        h.check_left(r)
        h.set(r, 8 + w0 + 16, 23 + i * (h.CH + 4), LEAD + h.n(r).text_w, h.CH)
    h.set("groupBox1", 8, 39, R - 8, 23 + 2 * h.CH + 4 + 8)
    y = 39 + h.n("groupBox1").nh + 8
    LW = 152
    xl, xa, xr = 8, 8 + LW + 8, 8 + LW + 8 + 32 + 8
    xb = xr + LW + 8
    ch = max(h.note_h("label2", LW), h.note_h("label3", LW))
    for cap, x in (("label2", xl), ("label3", xr)):
        h.text_align(cap, "BottomLeft")
        h.set(cap, x, y, LW, ch)
    y += ch + 2
    lh = 139
    h.set("lbFields", xl, y, LW, lh)
    h.set("lbTableColumns", xr, y, LW, lh)
    for i, b in enumerate(("bRight", "bAllRight", "bLeft", "bAllLeft")):
        h.set(b, xa, y + 32 * i, 32, 28)
    h.set("bUp", xb, y, 48, 28)
    h.set("bDown", xb, y + 32, 48, 28)
    y += lh + 8
    gh = h.note_h("label4", LW)
    h.text_align("label4", "BottomLeft")
    h.set("label4", xl, y, LW, gh)
    y += gh + 2
    h.set("cbGroupColumn", xl, y, LW, h.H)
    h.check_left("chkGrandTotals")
    h.set("chkGrandTotals", xr, y + (h.H - h.CH) // 2, LEAD + h.n("chkGrandTotals").text_w, h.CH)
    y += h.H + s.SECTION
    h.set(".", None, None, W0, y + 23 + 8)
    s.bar(h, right=("bOK", "bCancel"))
