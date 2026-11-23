"""FormMessagingSetup on the grid (UI-12 slice 8, layout script on the planner's tree). The three lists stood from x 28 with 36 px
empty under them and the Add, Up and Down column 118 px under their top: the lists from the padding 8 px apart down to the bottom
padding (Users and Extras keep their width, Messages to 8 px before the button column), their captions 2 px over them; the button column
on the right padding from the lists' top, 4 px apart."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    bw = h.n("butAdd").nw
    xb = R - bw
    y = h.n("listToFrom").ny
    for name in ("butAdd", "butUp", "butDown"):
        h.set(name, xb, y)
        y += h.n(name).nh + 4
    top = h.n("listToFrom").ny
    x = 8
    for cap, lst in (("label1", "listToFrom"), ("label4", "listExtras"), ("label5", "listMessages")):
        n = h.n(lst)
        w = n.nw if lst != "listMessages" else xb - 8 - x
        h.set(cap, x, None)
        h.set(lst, x, top, w, s.H(h) - 8 - top)
        x += w + 8
