"""FormCentralSecurity on the grid (UI-12 slice 12, layout script on the planner's tree; a FormODBase window of the Central Manager). Main
drew the Lock Date group with its Date and Days labels 210 and 225 px wide over their boxes, its note cut, and the three groups at three
heights; the planner stacked Domain Login over the other two and grew the window 132 px. The users and groups control (its own designer)
at the padding, 968 px wide (main's window width, 984); under it at the sibling gap three groups side by side at one top: Lock Date (the
note across its content and as tall as its lines, the Date and Days rows on one field column with the days note after the Days box, the
two check boxes as a list, the Central Manager note under them), Sync Options (the Sync Code caption over its box, the note, the three Push
buttons at the pitch of buttons with the push note beside Push Locks) at Lock Date's height, and Domain Login (the check box on the field
column over the Domain Path row) at its own height with OK and Close on the bottom right under it; the window as tall as the groups."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    Wd = 984
    R = Wd - s.PAD
    h.set("userControlSecurityTabs", s.PAD, s.PAD, R - s.PAD, None)
    u = h.n("userControlSecurityTabs")
    top = s.PAD + u.nh + s.GAP
    # Lock Date.
    w1 = 399
    c1 = w1 - 2 * s.PAD
    y = s.note(h, "label1", s.PAD, s.TITLE, c1, estimate=True) + s.ROWGAP
    fx = s.col_x(h, ["label3", "label4"])
    s.row(h, "label3", "textDate", y, fx)
    yb = s.row(h, "label4", "textDays", y + h.H + s.ROWGAP, fx)
    d = h.n("textDays")
    h.set("label2", d.nx + d.nw + s.LGAP, yb - h.H, h.lw("label2"), h.H)
    h.text_align("label2", "MiddleLeft")
    y = yb + s.ROWGAP
    for name in ("checkAdmin", "checkEnable"):
        h.check_left(name)
        h.text_align(name, "MiddleLeft")
        h.set(name, s.PAD, y, h.cw(name), h.CH)
        y += h.CH + s.ROWGAP
    y = s.note(h, "label6", s.PAD, y, c1, estimate=False)
    gh = y + s.PAD
    h.set("groupBox1", s.PAD, top, w1, gh)
    # Sync Options.
    x2 = s.PAD + w1 + s.GAP
    w2 = 273
    c2 = w2 - 2 * s.PAD
    y = h.captioned("label5", "textSyncCode", s.PAD, s.TITLE, c2) + s.ROWGAP
    h.set("textSyncCode", None, None, 122, None)
    y = s.note(h, "label7", s.PAD, y, c2, estimate=True) + s.GAP
    for name in ("butPushUsers", "butPushLocks", "butPushBoth"):
        h.set(name, s.PAD, y, 75, 24)
        y += 24 + s.ROWGAP
    pl = h.n("butPushLocks")
    nw = c2 - 75 - s.LGAP
    nh = h.note_h("label8", nw, estimate=True)
    h.set("label8", s.PAD + 75 + s.LGAP, pl.ny + 12 - nh // 2, nw, nh)
    gh = max(gh, y - s.ROWGAP + s.PAD)
    h.set("groupBox2", x2, top, w2, gh)
    h.set("groupBox1", None, None, None, gh)
    # Domain Login.
    x3 = x2 + w2 + s.GAP
    w3 = R - x3
    c3 = w3 - 2 * s.PAD
    fx3 = s.col_x(h, ["labelDomainPath"])
    h.check_left("checkDomainLoginEnabled")
    h.set("checkDomainLoginEnabled", fx3, s.TITLE, h.cw("checkDomainLoginEnabled"), h.CH)
    yb = s.row(h, "labelDomainPath", "textDomainLoginPath", s.TITLE + h.CH + s.ROWGAP, fx3, s.PAD + c3 - fx3)
    h.set("groupBox3", x3, top, w3, yb + s.PAD)
    Hd = top + gh + s.PAD
    s.size(h, Wd, Hd)
    h.set("butClose", R - 75, Hd - s.PAD - 24, 75, 24)
    h.set("butOK", R - 75 - s.GAP - 75, Hd - s.PAD - 24, 75, 24)
