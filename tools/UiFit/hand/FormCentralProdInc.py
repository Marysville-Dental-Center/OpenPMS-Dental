"""FormCentralProdInc on the grid (UI-12 slice 12, layout script on the planner's tree). The planner left Today's Date, the Date Range
group and the Report Type group on three left edges, the arrows row on none of the field columns and the write-offs note cut ("manua").
Today's Date on the first row; the Date Range group under it from the padding to the right padding, its From and To rows on the window's
field column with the arrows row over them on it; under it the Report Type and Show Insurance Write-offs groups side by side at one top and
one height (Report Type's radio buttons as a list; the two write-off radio buttons and their note), Include Unearned under the write-offs
group on its content edge; OK and Cancel on the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def radios(h, names, y):
    for name in names:
        h.check_left(name)
        # Fix round: the write-off radio buttons kept main's TopLeft circle and text; Windows draws a TopLeft text a few px low, and at
        # 150% its descenders were cut at the bottom of the 18 px row (Using, payment): centred in the row as the Report Type radios.
        if h.n(name).check_align == "TopLeft":
            h.n(name).new_check_align = "MiddleLeft"
            h.text_align(name, "MiddleLeft")
        h.set(name, s.PAD, y, h.cw(name), h.CH)
        y += h.CH + s.ROWGAP
    return y - s.ROWGAP


def layout(h):
    R = s.W(h) - s.PAD
    fxw = s.col_x(h, ["label4"])
    yb = s.row(h, "label4", "textToday", s.PAD, fxw)
    gy = yb + s.GAP
    fx = fxw - s.PAD  # the same column in the group
    x = fx
    for name in ("butLeft", "butThis", "butRight"):
        b = h.n(name)
        h.set(name, x, s.TITLE, None, 24)
        x += b.nw + s.LGAP
    yb = s.rows(h, [("label2", "textDateFrom"), ("label3", "textDateTo")], s.TITLE + 24 + s.ROWGAP, fx)
    h.set("groupBox2", s.PAD, gy, R - s.PAD, yb + s.PAD)
    top = gy + yb + s.PAD + s.GAP
    w1 = 123
    yb1 = radios(h, ["radioDaily", "radioMonthly", "radioAnnual", "radioProvider"], s.TITLE)
    yb3 = radios(h, ["radioWriteoffPay", "radioWriteoffProc"], s.TITLE) + s.ROWGAP
    x2 = s.PAD + w1 + s.GAP
    w2 = R - x2
    yb3 = s.note(h, "label5", s.PAD, yb3, w2 - 2 * s.PAD, estimate=False)
    gh = max(yb1, yb3) + s.PAD
    h.set("groupBox1", s.PAD, top, w1, gh)
    h.set("groupBox3", x2, top, w2, gh)
    h.check_left("checkShowUnearned")
    h.set("checkShowUnearned", x2 + s.PAD, top + gh + s.GAP, h.cw("checkShowUnearned"), h.CH)
    s.bar(h, right=("butOK", "butCancel"), bh=24)
