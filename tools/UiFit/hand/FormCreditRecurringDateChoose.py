"""FormCreditRecurringDateChoose on the grid (UI-12 slice 8, layout script on the planner's tree). The two month buttons stood at x 17
and 14 (and their lines at 119 and 116), the month combo hidden in the designer drawn over Last Month and the no-dates note at 45: the
message, the buttons and the combo on the padding; the Last Month and This Month rows on the 28 px pitch of 24 px buttons, their lines 6
px after the buttons; the combo (shown instead of the buttons when the card has earlier charge dates) on the first row with its caption 6
px after it, the no-dates note (shown instead of everything) on the first row. The lines take the code's widest text ("Cannot make
payment before start date: " and the start date's DateTime text), so the window grows from main's 388 to fit it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    h.set("labelMessage", 8, 8)
    y1 = 8 + h.n("labelMessage").nh + 4
    y2 = y1 + 24 + 4
    bw = h.n("butLastMonth").nw
    xl = 8 + bw + 6
    lw = h.lw("labelLastMonth")
    for but, lab, y in (("butLastMonth", "labelLastMonth", y1), ("butThisMonth", "labelThisMonth", y2)):
        h.set(but, 8, y, bw, 24)
        h.set(lab, xl, y, lw, 24)
    R = xl + lw
    cb = h.n("comboBoxMonthSelect")
    h.set("comboBoxMonthSelect", 8, y1, None, None)
    xm = 8 + cb.nw + 6
    h.set("labelMonthSelect", xm, y1, h.lw("labelMonthSelect"), cb.nh)
    h.set("labelNoDates", 8, y1, R - 8, None)
    if R + 8 > s.W(h):
        s.size(h, R + 8, None)
    s.bar(h, right=("butOK",))
