"""The report designer's DialogToolOptions on the grid (UI-12 slice 13, layout script on the planner's tree; its designer code is in
DialogToolOptions.cs, read and edited in InitializeComponent's body). The planner grew the window to 535 x 317 and left OK, Cancel and Apply
88 px short of the right padding, the stock check boxes 4 to 8 px too narrow for their text (cut), and the pages' rows at three left
edges. Main's 466 x 304; the tab control inside the padding down to the section gap over OK, Cancel and Apply on the bar. General: Display
with its box and its note, the Help System URL caption over its box, the three check boxes as a list from the content edge. Toolbar: the
captions over the two lists, the arrows between them, Up, Down and Reset in a column after them, the lists down to the page's padding.
Desktop Server: the note over the width, Port and Directory as label + box rows (Browse after the box), Restrict access, the warning note."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


CB = 24  # a stock check box's box and gap before its text (GDI+ measures the text a few px short)


def chk(h, name, x, y, w=None):
    h.check_left(name)
    h.set(name, x, y, w or CB + h.n(name).text_w, h.CH)


def lab(h, name, x, y, w):
    h.text_align(name, "MiddleRight")
    h.set(name, x, y, w, h.H)


def layout(h):
    W0, H0 = 466, 304
    h.set(".", None, None, W0, H0)
    top = s.bar(h, right=("bOK", "bCancel", "bApply"))
    tw, th = W0 - 16, top - s.SECTION - 8
    h.set("tabControl1", 8, 8, tw, th)
    pw, ph = tw - 8, th - 28
    R = pw - 8
    # General
    lw = h.lw("label1")
    lab(h, "label1", 8, 8, lw)
    h.set("tbRecentFilesMax", 8 + lw + 6, 8, 32, h.H)
    nx = 8 + lw + 6 + 32 + 6
    nh = h.note_h("label2", R - nx)
    h.text_align("label2", "TopLeft")
    h.set("label2", nx, 8 + 3, R - nx, nh)
    y = 8 + max(h.H, nh + 3) + 8
    h.text_align("label3", "BottomLeft")
    h.set("label3", 8, y, R - 8, 17)
    y += 17 + 2
    h.set("tbHelpUrl", 8, y, R - 8, h.H)
    y += h.H + 8
    for c in ("cbEditLines", "cbOutline", "cbTabInterface"):
        chk(h, c, 8, y)
        y += h.CH + 4
    # Toolbar
    lw2 = 152
    for cap, lst, x in (("label4", "lbOperation", 8), ("label5", "lbToolbar", 8 + lw2 + 8 + 23 + 8)):
        h.text_align(cap, "BottomLeft")
        h.set(cap, x, 8, lw2, 17)
        h.set(lst, x, 27, lw2, ph - 8 - 27)
    ax = 8 + lw2 + 8
    h.set("bCopyItem", ax, 27, 23, h.H)
    h.set("bRemove", ax, 54, 23, h.H)
    bx = ax + 23 + 8 + lw2 + 8
    for i, b in enumerate(("bUp", "bDown", "bReset")):
        h.set(b, bx, 27 + 27 * i, 53, h.H)
    # Desktop Server
    n7 = h.note_h("label7", R - 8)
    h.text_align("label7", "TopLeft")
    h.set("label7", 8, 8, R - 8, n7)
    y = 8 + n7 + 8
    lw3 = max(h.lw("label6"), h.lw("label9"))
    lab(h, "label6", 8, y, lw3)
    h.set("tbPort", 8 + lw3 + 6, y, 48, h.H)
    y += 27
    lab(h, "label9", 8, y, lw3)
    h.set("bBrowse", R - 26, y, 26, h.H)
    h.set("tbDirectory", 8 + lw3 + 6, y, R - 26 - 6 - (8 + lw3 + 6), h.H)
    y += h.H + 8
    chk(h, "ckLocal", 8 + lw3 + 6, y)
    y += h.CH + 8
    n8 = h.note_h("label8", R - (8 + lw3 + 6))
    h.text_align("label8", "TopLeft")
    h.set("label8", 8 + lw3 + 6, y, R - (8 + lw3 + 6), n8)
