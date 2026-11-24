"""The report designer's DialogAbout on the grid (UI-12 slice 15, layout script; designer code in DialogAbout.cs). Main and the planner cut
"E-mail:" to two lines (the two links on one row need 465 px of the 450). The logo with the name and versions beside it as on main; the
two links one under the other in a label column (Website, E-mail), the licence box over the width down to a section gap over the bar, OK
bottom right; main's 466 x 304. Fix round: the version lines as wide as the title, centred under it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    W0 = 466
    R = W0 - 8
    h.set(".", None, None, W0, 304)
    top = s.bar(h, right=("bOK",))
    lw = max(h.lw("label6"), h.lw("label5"))
    fx = 8 + lw + s.ITEM
    for lb, link, y in (("label6", "linkLabel4", 88), ("label5", "linkLabel3", 115)):
        s.lab(h, lb, 8, y, lw)
        h.text_align(link, "MiddleLeft")
        h.set(link, fx, y, h.n(link).text_w + 8, h.H)
    # Fix round: the version lines centred under the title (its x and width; they kept narrower widths when they moved to its x).
    t = h.n("label8")
    for v in ("lVersion", "lVMVersion"):
        h.set(v, t.nx, None, t.nw, None)
    y = 115 + h.H + 8
    h.set("tbLicense", 8, y, R - 8, top - s.SECTION - y)
