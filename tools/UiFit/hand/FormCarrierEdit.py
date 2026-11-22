"""FormCarrierEdit (Edit Carrier) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The planner kept the
Appt Text Back Color label and its colour picker as drawn (the picker over the label's end), left Is Trusted for Real-Time Eligibility at the
content edge under Hidden on the field column, and kept the In Use By group at main's x: the script puts both check boxes on the field
column, the colour label right-aligned 6 px before its picker on the field column, the Import Benefit Coinsurance group under them on the
field column to the fields' right edge, and the In Use By group on the window's content edge to the fields' right edge with its fields on the
window's field column. Outside Canada FormCarrierEdit sets the window 560 px wide (Load): everything but the CDAnet group stays within the
fields' right edge (502)."""


def layout(h):
    H = h.H
    F = h.n("textCarrierName").x
    R = h.n("textAddress").x + h.n("textAddress").w
    y = h.n("checkIsHidden").y
    for name in ("checkIsHidden", "checkRealTimeEligibility"):
        h.check_left(name)
        h.set(name, F, y, h.cw(name), h.CH)
        y += h.CH + 4
    lw = F - 6 - 8
    h.set("labelColor", 8, y, lw, H)
    h.text_align("labelColor", "MiddleRight")
    h.set("odColorPickerBack", F, y + 1, 74, 21)
    yc = y + H + 4
    g2 = h.box("groupBox2", F, yc, R - F)
    g2.checks(("radioBenefitSendsPat", "radioBenefitSendsIns"), g2.left)
    g2.end()
    y1 = yc + h.n("groupBox2").nh + 8
    g1 = h.n("groupBox1")
    # What stands under the In Use By group moves down with it; the window grows by as much.
    dy = y1 - g1.y
    for k in h.tree.children:
        if k is not g1 and k.y >= g1.y and k.dock in ("None", ""):
            h.set(k.name, None, k.y + dy)
    h.set(".", None, None, h.tree.w, h.tree.h + dy)
    h.set("groupBox1", 8, y1, R - 8, g1.h)
    fi = F - 8
    l9 = h.lw("label9")
    h.set("label9", fi - 6 - l9, None, l9)
    h.set("textPlans", fi, None, 36)
    h.set("comboPlans", fi + 36 + 6, None, R - 8 - 8 - (fi + 36 + 6))
