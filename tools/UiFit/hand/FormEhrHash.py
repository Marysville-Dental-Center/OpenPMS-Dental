"""FormEhrHash on the grid (UI-12 slice 8, layout script on the planner's tree). The "( uses SHA-1 )" note (anchored to the bottom left)
was drawn over Generate Hash, the message box ended 8 px short of the right padding and Transmit and the Hash box 4 px short of it: the
note 6 px after Generate Hash on the bottom bar's row, Transmit on the right padding, the Hash box 8 px over the bar ending on the right
padding with its caption 6 px before it, the message box from the padding to the right padding and down to 8 px over the Hash row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    yb = s.bar(h, left=("butGererate",), right=("butTransmit",))
    g = h.n("butGererate")
    h.text_align("label2", "MiddleLeft")
    h.set("label2", 8 + g.nw + 6, yb, h.lw("label2"), g.nh)
    th = h.n("textHash")
    yh = yb - 8 - th.nh
    h.set("textHash", None, yh, R - th.nx, None)
    l3 = h.n("label3")
    h.set("label3", th.nx - 6 - l3.nw, yh, None, th.nh)
    tm = h.n("textMessage")
    h.set("textMessage", 8, None, R - 8, yh - 8 - tm.ny)
