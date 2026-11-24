"""FormCentralUserEdit on the grid (UI-12 slice 12, layout script on the planner's tree). The planner left the tab control at main's 15
px margin, the user page's name box and group list 22 px in and the domain user row 16 px short of the page's edge, and grew the window 4
px: back to main's 610 x 551. Is Hidden on the first row; the tab control from the padding to the right padding under it down to the
section gap over the bar; on the User page the name box over the group list from the page's padding, the security tree (its own designer)
beside them to the page's right padding, the Domain User row over it ending there; on the Alert Subs page the caption 2 px over its list
from the page's padding. Change Password and Unlock Account on the left of the bar, OK and Cancel on the right. The Name and User Group
labels stand under the tab control as on main (no code shows them; grid-known)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 610, 551)
    R = 610 - s.PAD
    yb = s.bar(h, left=("butPassword", "butUnlock"), right=("butOK", "butCancel"), bh=24)
    h.check_left("checkIsHidden")
    h.set("checkIsHidden", s.PAD, s.PAD, h.cw("checkIsHidden"), h.CH)
    top = s.PAD + h.CH + s.GAP
    th = yb - s.SECTION - top
    h.set("tabControl", s.PAD, top, R - s.PAD, th)
    pw = (R - s.PAD) - 4  # the page inside the tab control (2 px borders), under its 21 px tab row
    ph = th - 23
    for p in ("tabUser", "tabAlertSubs"):
        h.set(p, None, None, pw, ph)
    pr = pw - s.PAD
    pb = ph - s.PAD
    lw = 197
    h.set("textUserName", s.PAD, s.PAD, lw, h.H)
    lt = s.PAD + h.H + s.ROWGAP
    h.set("listUserGroup", s.PAD, lt, lw, pb - lt)
    tx = s.PAD + lw + s.GAP
    h.set("securityTreeUser", tx, lt, pr - tx, pb - lt)
    b = h.n("butPickDomainUser")
    h.set("butPickDomainUser", pr - b.nw, s.PAD, None, h.H)
    t = h.n("textDomainUser")
    h.set("textDomainUser", pr - b.nw - s.LGAP - t.nw, s.PAD, None, h.H)
    s.gap_before(h, "labelDomainUser", "textDomainUser")
    ly = h.captioned("label7", "listAlertSubMulti", s.PAD, s.PAD, 250, h=pb - (s.PAD + 17 + s.CAPGAP))
