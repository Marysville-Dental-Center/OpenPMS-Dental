"""The report designer's StyleCtl on the grid (UI-12 slice 13, layout script on the planner's tree; its designer code is in StyleCtl.cs,
read and edited in InitializeComponent's body). PropertyDialog docks it in its tab page (the control keeps main's size). The planner left
the Padding group's left labels 2 px in from its content edge and its right boxes at two left edges (266 and 278), and the XML labels 7 to
11 px in. Padding: the left labels from the content edge, the boxes 128 px with their fx buttons, the right band 16 px after them with one
label column (Bottom) and one box column; the groups to 448 px (the page's width); XML: the labels from the content edge, Data Element Name's box to the content's right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    # the three groups from the padding to 448 (PropertyDialog's page is about 450 px wide; the planner spanned them to 464)
    gw = 448 - 8
    for i, g in enumerate(("groupBox1", "grpBoxPadding", "gbXML")):
        h.set(g, 8, None, gw, None)
    lwl = max(h.lw(n) for n in ("label11", "label13"))
    lwr = max(h.lw(n) for n in ("label12", "label14"))
    fx = 8 + lwl + 6
    rx = fx + 128 + 6 + 22 + 16
    rfx = rx + lwr + 6
    for i, (ll, lt, lb, rl, rt, rb) in enumerate((("label11", "tbPadLeft", "bValueExpr", "label12", "tbPadRight", "button3"),
                                                   ("label13", "tbPadTop", "button1", "label14", "tbPadBottom", "button2"))):
        y = 23 + 27 * i
        for lab, x, lw in ((ll, 8, lwl), (rl, rx, lwr)):
            h.text_align(lab, "MiddleRight")
            h.set(lab, x, y, lw, h.H)
        h.set(lt, fx, y, 128, h.H)
        h.set(lb, fx + 128 + 6, y, 22, h.H)
        h.set(rt, rfx, y, 128, h.H)
        h.set(rb, rfx + 128 + 6, y, 22, h.H)
    lw = max(h.lw(n) for n in ("label1", "label2"))
    xfx = 8 + lw + 6
    for i, (lab, f) in enumerate((("label1", "tbDEName"), ("label2", "cbDEOutput"))):
        h.text_align(lab, "MiddleRight")
        h.set(lab, 8, 23 + 27 * i, lw, h.H)
        h.set(f, xfx, 23 + 27 * i, (gw - 8 - xfx) if f == "tbDEName" else None, h.H)
