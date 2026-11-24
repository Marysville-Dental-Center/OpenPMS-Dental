"""FormMySQLUserManager on the grid (UI-12 slice 12, layout script on the planner's tree). The planner grew the window 49 px for main's
26 px list margin: back to main's 624 x 498: the connection group from the padding to the right padding with its two columns of captioned
boxes (Server Name and MySQL User, Port and MySQL Password), Connect under them on its content edge; the Users caption and list from the
padding down to the bottom padding, Add, Edit and Drop beside the list's top at the pitch of buttons, Close on the bottom right."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 624, 498)
    R = 624 - s.PAD
    B = 498 - s.PAD
    gw = R - s.PAD
    cw = gw - 2 * s.PAD
    colw = (cw - s.GAP) // 2
    x2 = s.PAD + colw + s.GAP
    y1 = h.captioned("label1", "textServer", s.PAD, s.TITLE, colw)
    h.captioned("label3", "textUser", x2, s.TITLE, colw)
    y = y1 + s.ROWGAP
    y2 = h.captioned("label5", "textPort", s.PAD, y, colw)
    h.set("textPort", None, None, 72, None)
    h.captioned("label2", "textPassword", x2, y, colw)
    h.set("butConnect", s.PAD + cw - 75, y2 + s.ROWGAP, 75, 24)
    gh = y2 + s.ROWGAP + 24 + s.PAD
    h.set("groupConnection", s.PAD, s.PAD, gw, gh)
    cy = s.PAD + gh + s.GAP
    lw = 235
    h.set("label4", s.PAD, cy, h.lw("label4"), 17)
    h.text_align("label4", "BottomLeft")
    ly = cy + 17 + s.CAPGAP
    h.set("listBoxUsers", s.PAD, ly, lw, B - ly)
    by = ly
    for name in ("butAdd", "butEdit", "butDrop"):
        h.set(name, s.PAD + lw + s.GAP, by, 75, 24)
        by += 24 + s.ROWGAP
    h.set("butClose", R - 75, B - 24, 75, 24)
