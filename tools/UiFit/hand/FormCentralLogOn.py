"""FormCentralLogOn on the grid (UI-12 slice 12, layout script on the planner's tree; its designer code is in the window's own .cs file).
Main drew the user list 31 px in, 32 px over the bottom and Exit 8 px past OK's edge: the User caption and list from the padding, the
list down to the bottom padding (the user name box, hidden in the designer and shown by the code in place of the list, on the list's first
row); the Password caption and box beside them, the box to the right padding, OK under it on the right padding; Exit on the bottom
right."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - s.PAD
    B = s.H(h) - s.PAD
    lw = 180
    yb = h.captioned("label1", "listUser", s.PAD, s.PAD, lw, h=B - (s.PAD + 17 + s.CAPGAP))
    top = h.n("listUser").ny
    h.set("textUser", s.PAD, top, lw, h.H)
    x2 = s.PAD + lw + s.GAP
    yp = h.captioned("label2", "textPassword", x2, s.PAD, R - x2)
    h.set("butOK", R - 75, yp + s.ROWGAP, 75, 24)
    s.bar(h, right=("butCancel",), bh=24)
