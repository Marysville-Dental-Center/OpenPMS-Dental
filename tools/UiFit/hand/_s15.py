"""Common steps of the UI-12 slice 15 layout scripts (the report designer's and viewer's remaining dialogs and property pages: plain
Forms and UserControls with their designer code in their .cs files, read and edited in InitializeComponent's body), on slice 13's
_s13.py (bar, label_column, stack): a label right-aligned before its field, a row of label + field + buttons, a stock check box or radio
button with its 24 px lead (the box and its gap: GDI+ measures the stock box's text short, slice 13), a caption over its box, and the
property pages' width (PropertyDialog docks a page in a tab page of about 450 x 300 px: the content ends at 440, the page's padding)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)

PAD = 8
GAP = 8
SECTION = 12
ITEM = 6  # the item gap: a field's button (fx, "...") 6 px after it
BAND = 16  # the band gap between two columns of a row
FX = 22  # the report designer's fx and "..." buttons
PAGE_R = 440  # a property page's content edge: the tab page is about 450 px wide (slice 13 kept every page within 448)
PAGE_B = 292  # and about 300 px tall
W = s.W
H = s.H
bar = s.bar


def lab(h, name, x, y, w, align="MiddleRight"):
    """A label one field tall on the row at y, its text aligned (right-aligned before its field)."""
    h.text_align(name, align)
    h.set(name, x, y, w, h.H)


def row(h, label, field, x, lw, fw, y, after=(), fh=None):
    """The label right-aligned in [x, x+lw), the field 6 px after it fw wide (fh tall, default the field height), each button of after
    6 px after the field (its width kept). Returns the right edge of the row."""
    lab(h, label, x, y, lw)
    fx = x + lw + ITEM
    h.set(field, fx, y, fw, fh or h.H)
    r = fx + fw
    for b in after:
        bw = h.n(b).nw
        h.set(b, r + ITEM, y, bw, h.H)
        r += ITEM + bw
    return r


def check_w(h, name):
    """A stock check box's or radio button's width: the box and its gap (24 px) and its text."""
    return 24 + (h.n(name).text_w or 0)


def chk(h, name, x, y):
    """A stock check box or radio button on the row at y (centred on the field height), the box on the left. Returns its right edge."""
    h.check_left(name)
    w = check_w(h, name)
    h.set(name, x, y + (h.H - h.CH) // 2, w, h.CH)
    return x + w


def caption(h, name, x, y, w):
    """A caption 17 px tall over the box that follows 2 px under it (BottomLeft). Returns the box's top."""
    h.text_align(name, "BottomLeft")
    h.set(name, x, y, w, 17)
    return y + 17 + 2


def note(h, name, x, y, w):
    """A wrapping label as tall as its lines at w (TopLeft). Returns its bottom."""
    nh = h.note_h(name, w)
    h.text_align(name, "TopLeft")
    h.set(name, x, y, w, nh)
    return y + nh
