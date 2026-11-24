"""The report designer's StyleTextCtl on the grid (UI-12 slice 13, layout script on the planner's tree; its designer code is in
StyleTextCtl.cs, read and edited in InitializeComponent's body). PropertyDialog docks it in a 450 px tab page (the control keeps main's
456 x 280). The planner left the right band's boxes at three left edges (296, 304 and 320) and the font dialog's "..." past the Font group's
padding. Two bands on the window's columns, the Font group's rows on the same columns: the left labels right-aligned before 80 px boxes,
each box's fx button 6 px after it and the colour and font dialogs' "..." after that (the font dialog on the Family row, beside Color's);
the right band 16 px after them, its labels in one column (Writing Mode the widest), its boxes 88 px with their fx buttons; Value and
Format over the whole width with fx on the right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)

FX = 22


def row(h, label, combo, fx, x0, lw, fw, y, more=None, dx=0):
    """label right-aligned in [x0, x0+lw), the combo 6 px after it fw wide, fx 6 px after, more 6 px after fx (x in the parent: dx is the
    parent's x in the window)."""
    fx_x = x0 + lw + 6
    h.text_align(label, "MiddleRight")
    h.set(label, x0 - dx, y, lw, h.H)
    h.set(combo, fx_x - dx, y, fw, h.H)
    h.set(fx, fx_x + fw + 6 - dx, y, FX, h.H)
    if more:
        h.set(more, fx_x + fw + 6 + FX + 6 - dx, y, FX, h.H)


def layout(h):
    W0 = s.W(h)
    left = ("lblValue", "lFont", "label3", "label9", "label6", "label7", "label4")
    right = ("label11", "label10", "label2", "label5", "label8")
    lwl = max(h.lw(n) for n in left)
    lwr = max(h.lw(n) for n in right)
    fl, fr = 80, 88
    rx = 8 + lwl + 6 + fl + 6 + FX + 6 + FX + 16
    # Value
    row(h, "lblValue", "cbValue", "bValueExpr", 8, lwl, W0 - 8 - FX - 6 - (8 + lwl + 6), 8)
    # Font
    g = 8
    h.set("groupBox1", 8, 39, W0 - 16, None)
    # Fix round (review of slice 13): the Font group's labels from its content edge (x 16 in the window, 8 in the group; they started at
    # the group's x 0 and hid its left border between the rows), 8 px narrower so the boxes keep their column.
    assert max(h.lw(n) for n in ("lFont", "label3", "label9")) <= lwl - 8, "StyleTextCtl: a Font group label is wider than its column"
    row(h, "lFont", "cbFontFamily", "bFamily", 16, lwl - 8, fl, 23, "bFont", dx=g)
    row(h, "label3", "cbFontStyle", "bStyle", 16, lwl - 8, fl, 50, dx=g)
    row(h, "label9", "cbColor", "bColorEx", 16, lwl - 8, fl, 77, "bColor", dx=g)
    row(h, "label11", "cbFontSize", "bSize", rx, lwr, fr, 23, dx=g)
    row(h, "label10", "cbFontWeight", "bWeight", rx, lwr, fr, 50, dx=g)
    row(h, "label2", "cbTextDecoration", "button2", rx, lwr, fr, 77, dx=g)
    h.set("groupBox1", None, None, None, 77 + h.H + 8)
    y = 39 + h.n("groupBox1").nh + 8
    row(h, "label6", "cbHorzAlign", "bAlignment", 8, lwl, fl, y)
    row(h, "label5", "cbVerticalAlign", "bVertical", rx, lwr, fr, y)
    row(h, "label7", "cbDirection", "bDirection", 8, lwl, fl, y + 27)
    row(h, "label8", "cbWritingMode", "bWrtMode", rx, lwr, fr, y + 27)
    row(h, "label4", "cbFormat", "bFormat", 8, lwl, W0 - 8 - FX - 6 - (8 + lwl + 6), y + 54)
