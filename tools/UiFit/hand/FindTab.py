"""The report designer's FindTab (the Find, Replace and GoTo window) on the grid (UI-12 slice 13, layout script on the planner's tree; its
designer code is in FindTab.cs, read and edited in InitializeComponent's body). The planner left the tab control 21 px short of the right
padding and 37 px short of the bottom, the buttons of the Replace page at two left edges (326 and 344), Find Next and Replace beside the
boxes, and the Find page's labels at another column than the other pages'. The tab control fills the window inside the padding; on each
page one label column from the content edge (Replace With, Line Number), the boxes on one field column up to 16 px before one column of
buttons on the page's right padding (75 px each, 4 px apart); Match Case on the field column; Search Direction under it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def lab(h, name, y, w):
    h.text_align(name, "MiddleRight")
    h.set(name, 8, y, w, h.H)


def layout(h):
    W0, H0 = s.W(h), s.H(h)
    h.set("tcFRG", 8, 8, W0 - 16, H0 - 16)
    pw = W0 - 16 - 8
    bx = pw - 8 - 75
    lw = max(h.lw(n) for n in ("label1", "label3", "label2", "label4"))
    fx = 8 + lw + 6
    fw = bx - 16 - fx
    for col in (("btnNext", "btnCancel"), ("btnFindNext", "btnReplace", "btnReplaceAll", "bCloseReplace"), ("btnGoto", "bCloseGoto")):
        for i, b in enumerate(col):
            h.set(b, bx, 8 + 27 * i, 75, h.H)
    lab(h, "label1", 8, lw)
    h.set("txtFind", fx, 8, fw, h.H)
    h.check_left("chkCase")
    h.set("chkCase", fx, 35 + (h.H - h.CH) // 2, None, h.CH)
    h.set("radioUp", 8, 23, None, h.CH)
    h.set("radioDown", 8 + h.n("radioUp").nw + 16, 23, None, h.CH)
    h.set("groupBox1", fx, 35 + h.H + 8, 8 + h.n("radioUp").nw + 16 + h.n("radioDown").nw + 8, 23 + h.CH + 8)
    lab(h, "label3", 8, lw)
    h.set("txtFindR", fx, 8, fw, h.H)
    lab(h, "label2", 35, lw)
    h.set("txtReplace", fx, 35, fw, h.H)
    h.check_left("chkMatchCase")
    h.set("chkMatchCase", fx, 62 + (h.H - h.CH) // 2, None, h.CH)
    lab(h, "label4", 8, lw)
    h.set("txtLine", fx, 8, 100, h.H)
