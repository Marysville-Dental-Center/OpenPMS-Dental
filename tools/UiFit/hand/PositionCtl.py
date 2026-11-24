"""The report designer's PositionCtl on the grid (UI-12 slice 13, layout script on the planner's tree; its designer code is in
PositionCtl.cs, read and edited in InitializeComponent's body). PropertyDialog docks it in a 450 px tab page. The planner kept the two
groups at main's x 24 with 64 px of empty room on their right, the Hide Duplicates label over its box and the XML Element Style label on
two lines (cut). The groups from the padding over the content's width; Name and Position's left boxes on one field column, Span Table
Columns and Top/Height as the right band 16 px after; in Text Processing the two check boxes and Hide Duplicates on one row (each 16 px after
the one before), XML Element Style and Toggle Image as label + box rows, Toggle Image's box to the content's right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def lab(h, name, x, y, w):
    h.text_align(name, "MiddleRight")
    h.set(name, x, y, w, h.H)


def layout(h):
    gw = 432
    # Position: Left/Width/Z Index | Top/Height
    lw = max(h.lw(n) for n in ("label5", "label8", "label9", "label1"))
    fx = 8 + lw + 6  # in the group; the window's column is 8 px further right
    rw = max(h.lw(n) for n in ("label6", "label7"))
    rx = fx + 72 + 16
    for i, (l1, t1, l2, t2) in enumerate((("label5", "tbLeft", "label6", "tbTop"), ("label8", "tbWidth", "label7", "tbHeight"),
                                           ("label9", "tbZIndex", None, None))):
        y = 23 + 27 * i
        lab(h, l1, 8, y, lw)
        h.set(t1, fx, y, 72, h.H)
        if l2:
            lab(h, l2, rx, y, rw)
            h.set(t2, rx + rw + 6, y, 72, h.H)
    h.set("gbPosition", 8, 39, gw, 23 + 2 * 27 + h.H + 8)
    # Name and Span Table Columns on the window's columns (the group's + 8)
    lab(h, "label1", 8, 8, lw + 8)
    h.set("tbName", 8 + fx, 8, 128, h.H)
    cw = h.lw("lblColSpan")
    sx = 8 + fx + 128 + 16
    lab(h, "lblColSpan", sx, 8, cw)
    h.set("tbColSpan", sx + cw + 6, 8, 48, h.H)
    # Text Processing
    y0 = 39 + h.n("gbPosition").nh + 8
    x = 8
    for c in ("chkCanGrow", "chkCanShrink"):
        h.check_left(c)
        w = 16 + h.n(c).text_w + 4
        h.set(c, x, 23 + (h.H - h.CH) // 2, w, h.CH)
        x += w + 16
    hw = h.lw("label2")
    lab(h, "label2", x, 23, hw)
    h.set("cbHideDuplicates", x + hw + 6, 23, gw - 8 - (x + hw + 6), h.H)
    tw = max(h.lw(n) for n in ("label3", "label4"))
    lab(h, "label3", 8, 50, tw)
    h.set("cbDataElementStyle", 8 + tw + 6, 50, 120, h.H)
    lab(h, "label4", 8, 77, tw)
    h.set("cbToggleImage", 8 + tw + 6, 77, gw - 8 - (8 + tw + 6), h.H)
    h.set("gbText", 8, y0, gw, 77 + h.H + 8)
