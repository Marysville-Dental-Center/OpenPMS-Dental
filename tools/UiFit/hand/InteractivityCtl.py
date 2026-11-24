"""The report designer's InteractivityCtl on the grid (UI-12 slice 13, layout script on the planner's tree; its designer code is in
InteractivityCtl.cs, read and edited in InitializeComponent's body). PropertyDialog docks it in a 450 px tab page. The planner put each
radio button's box 6 px after its own text (three left edges: 90, 109, 118) and Bookmark's label 35 px in from the padding. The groups to
the page's right padding (448); Action: the boxes on one column after the widest radio button, each fx and Drill Through's "..." and
Parameters ending on the content's right edge; Visibility: one label column from the content edge; Bookmark on Visibility's field column,
its fx on the right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def lab(h, name, x, y, w):
    h.text_align(name, "MiddleRight")
    h.set(name, x, y, w, h.H)


def layout(h):
    R = 448
    gw = R - 8
    cr = gw - 8  # the groups' content right edge
    rw = max(16 + h.n(n).text_w + 4 for n in ("rbNoAction", "rbHyperlink", "rbBookmarkLink", "rbDrillthrough"))
    bx = 8 + rw + 6
    for i, r in enumerate(("rbNoAction", "rbHyperlink", "rbBookmarkLink", "rbDrillthrough")):
        h.check_left(r)
        h.set(r, 8, 23 + 27 * i + (h.H - h.CH) // 2, rw, h.CH)
    for tb, fx, y in (("tbHyperlink", "bHyperlink", 50), ("tbBookmarkLink", "bBookmarkLink", 77)):
        h.set(fx, cr - 22, y, 22, h.H)
        h.set(tb, bx, y, cr - 22 - 6 - bx, h.H)
    pw = h.n("bParameters").nw
    h.set("bParameters", cr - pw, 104, pw, h.H)
    h.set("bDrillthrough", cr - pw - 6 - 24, 104, 24, h.H)
    h.set("tbDrillthrough", bx, 104, cr - pw - 6 - 24 - 6 - bx, h.H)
    h.set("groupBox1", 8, 8, gw, 104 + h.H + 8)
    y = 8 + h.n("groupBox1").nh + 8
    lw = max(h.lw(n) for n in ("label2", "label3"))
    fx = 8 + lw + 6
    lab(h, "label2", 8, 23, lw)
    h.set("bHidden", cr - 22, 23, 22, h.H)
    h.set("tbHidden", fx, 23, cr - 22 - 6 - fx, h.H)
    lab(h, "label3", 8, 50, lw)
    h.set("cbToggle", fx, 50, 152, h.H)
    h.set("grpBoxVisibility", 8, y, gw, 50 + h.H + 8)
    y += h.n("grpBoxVisibility").nh + 8
    lab(h, "label1", 8, y, lw + 8)
    h.set("bBookmark", R - 22, y, 22, h.H)
    h.set("tbBookmark", 8 + fx, y, R - 22 - 6 - (8 + fx), h.H)
