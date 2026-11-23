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
    # Fix round 8: the Canadian texts Load gives the City and Electronic ID labels (grid-runtime-texts.tsv) fit, right-aligned on the column.
    for name in ("labelCitySt", "labelElectID"):
        lw = h.lw(name)
        h.set(name, F - 6 - lw, None, lw)
    # The Supported Transaction Types group to the CDAnet group's content edge (the planner widened CDAnet to the window's content edge).
    cd, g3 = h.n("groupCDAnet"), h.n("groupBox3")
    h.set("groupBox3", None, None, cd.nw - 8 - g3.nx, None)
    # Fix round 8: the Carrier Group row (Load shows it when a CarrierGroupNames definition exists) on its main row under Consolidate Ortho Ins
    # Payments; the planner put the hidden row at the end of its column, under the CDAnet group, which Load hides outside Canada and takes off
    # the window's height (GridRulesTests.FormCarrierEdit_OutsideCanada_EveryControlFits). The rows under it move down one pitch.
    P = H + 4
    yo = h.n("comboOrthoConsolidate").ny + P
    bar = ("butSave", "butDelete")
    for k in h.tree.children:
        if k.dock in ("None", "") and k.name not in ("labelCarrierGroupName", "comboCarrierGroupName") + bar and k.ny >= yo:
            h.set(k.name, None, k.ny + P)
    lc = h.lw("labelCarrierGroupName")
    h.set("labelCarrierGroupName", F - 6 - lc, yo, lc, H)
    h.text_align("labelCarrierGroupName", "MiddleRight")
    h.set("comboCarrierGroupName", F, yo, h.n("comboCarrierGroupName").nw, H)
    # The bottom bar 12 px under the content (the CDAnet group), as the planner sets it, the window to its bottom padding.
    content = max(k.ny + k.nh for k in h.tree.children if k.dock in ("None", "") and k.visible and k.name not in bar)
    yb = content + 12
    for name in bar:
        h.set(name, None, yb)
    h.set(".", None, None, h.tree.nw, yb + h.n("butSave").nh + 8)
