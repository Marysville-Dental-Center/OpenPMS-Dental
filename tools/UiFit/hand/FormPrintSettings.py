"""FormPrintSettings on the grid (UI-12 slice 12, layout script on the planner's tree). The settings over the print preview sit in the first panel of
a split container, which the planner does not lay out: main's groups from x 10 with their labels 68 px wide touching their boxes. The three
groups from the padding, 8 px apart, at one top and one height, each with its two rows at the pitch and its labels right-aligned 6 px before
one field column; Export and Print beside them at the band gap on the groups' first and second rows, Landscape after Print; the panel as
tall as the groups and the padding. The preview (drawn by the print preview control) fills the second panel; Close ends on the right
padding of the strip under it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


GROUPS = (("groupBox1", (("labelWidth", "textWidth"), ("labelHeight", "textHeight"))),
          ("groupBox2", (("label5", "textMarginWidth"), ("label4", "textMarginHeight"))),
          ("groupBox3", (("label2", "textXPos"), ("label1", "textYPos"))))

def layout(h):
    top = s.PAD
    x = s.PAD
    for grp, rows_ in GROUPS:
        labels = [r[0] for r in rows_]
        fx = s.col_x(h, labels)
        yb = s.rows(h, rows_, s.TITLE, fx)
        w = fx + h.n(rows_[0][1]).nw + s.PAD
        h.set(grp, x, top, w, yb + s.PAD)
        x += w + s.GAP
    gh = h.n(GROUPS[0][0]).nh
    bx = x - s.GAP + 16
    h.set("butExport", bx, top + s.TITLE, None, h.H)
    h.set("butPrint", bx, top + s.TITLE + h.H + s.ROWGAP, None, h.H)
    p = h.n("butPrint")
    h.check_left("checkLandscape")
    h.set("checkLandscape", bx + p.nw + s.LGAP, p.ny + (h.H - h.CH) // 2, h.cw("checkLandscape"), h.CH)
    h.splitter("splitContainer", top + gh + s.PAD)
    W = h.n("splitContainer").nw
    h.set("butClose", W - s.PAD - 75, None)
