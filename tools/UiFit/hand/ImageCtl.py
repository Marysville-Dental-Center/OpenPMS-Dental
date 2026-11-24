"""The report designer's ImageCtl on the grid (UI-12 slice 15, layout script on the planner's tree; designer code in ImageCtl.cs). The
planner left the Source group's boxes at three left edges (86, 88, 102). The Source group over the page's width (8 to 440): the radio
buttons in one column, their boxes on one column 6 px after the widest, each box's fx and "..." on the group's content edge; Database's
MIME type box with its fx, its value box on the next row on the column; Sizing's box under the group on the same column, its label
right-aligned before it; main's 472 x 288 (a property page)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    h.set(".", None, None, 472, 288)
    gw = s.PAGE_R - 8
    cr = gw - 8
    rw = max(s.check_w(h, n) for n in ("rbExternal", "rbEmbedded", "rbDatabase"))
    bx = 8 + rw + s.ITEM
    more = cr - s.FX
    fxx = more - s.ITEM - s.FX
    bw = fxx - s.ITEM - bx
    for rb, box, fx, dots, y in (("rbExternal", "tbValueExternal", "bExternalExpr", "bExternal", 23),
                                 ("rbEmbedded", "cbValueEmbedded", "bEmbeddedExpr", "bEmbedded", 50)):
        s.chk(h, rb, 8, y)
        h.set(box, bx, y, bw, h.H)
        h.set(fx, fxx, y, s.FX, h.H)
        h.set(dots, more, y, s.FX, h.H)
    s.chk(h, "rbDatabase", 8, 77)
    h.set("cbMIMEType", bx, 77, 88, h.H)
    h.set("bMimeExpr", bx + 88 + s.ITEM, 77, s.FX, h.H)
    h.set("cbValueDatabase", bx, 104, bw, h.H)
    h.set("bDatabaseExpr", fxx, 104, s.FX, h.H)
    h.set("groupBox1", 8, 8, gw, 104 + h.H + 8)
    y = 8 + 104 + h.H + 8 + 8
    lw = h.lw("label1")
    s.row(h, "label1", "cbSizing", 8 + bx - s.ITEM - lw, lw, 96, y)
