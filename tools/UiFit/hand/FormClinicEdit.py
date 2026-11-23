"""FormClinicEdit (Clinic) on the grid (UI-12 slice 1, layout script of grid_hand.py). The planner parted the Time Zone label's second line
from its label, put the e-mail alias example over the e-mail box and the None button of Default Provider at the window's edge, and widened the
window to 1293 px; the script lays the window out in two columns: the clinic's fields on one field column (the check boxes' boxes on it, the
e-mail buttons and the alias example after their fields, the Time Zone note after its box, the hidden MedLab account row at the end of the
column), the address tabs, the Default Insurance Billing Provider group and the claim defaults on the right (the tabs' address rows at one
place on every page, so the fields do not jump when the page changes). The window grows to 1180 x 528."""


def layout(h):
    H = h.H
    CH = h.CH
    WW = 1180
    root = h.box(".")
    R = WW - 8
    # The left column.
    labels = ("label21", "labelExternalID", "label23", "label1", "label2", "label8", "label22", "label10", "labelClinicEmailAliasOverride",
              "label5", "labelSchedRules", "labelTimeZone", "labelMedLabAcctNum")
    lw = max(h.lw(x) for x in labels)
    F = 8 + lw + 6
    c = h.box(".")
    c.checks(("checkIsMedicalOnly",), F)
    c.field("label21", "textClinicNum", F, 156, lw=lw)
    c.field("labelExternalID", "textExternalID", F, 156, lw=lw)
    c.field("label23", "textClinicAbbr", F, 156, lw=lw)
    c.field("label1", "textDescription", F, 264, lw=lw)
    c.field("label2", "textPhone", F, 156, lw=lw, after=[("label9", None, h.lw("label9"))])
    c.field("label8", "textFax", F, 156, lw=lw, after=[("label6", None, h.lw("label6"))])
    c.field("label22", "comboRegion", F, 156, lw=lw)
    c.checks(("checkExcludeFromInsVerifyList", "checkProcCodeRequired"), F)
    c.field("label10", "textEmail", F, 264, lw=lw, after=[("butEmail", None, 24), ("butEmailNone", None, 48)])
    c.field("labelClinicEmailAliasOverride", "textClinicEmailAliasOverride", F, 156, lw=lw,
            after=[("labelEmailAliasExample", None, h.lw("labelEmailAliasExample"))])
    c.field("label5", "textBankNumber", F, 292, lw=lw)
    ys = c.y
    h.set("labelSchedRules", F - 6 - lw, ys, lw, H)
    h.text_align("labelSchedRules", "MiddleRight")
    h.set("textSchedRules", F, ys, 292, 60)
    c.y = ys + 60 + 4
    c.bottom_used = c.y - 4
    c.field("labelTimeZone", "comboBoxTimeZone", F, 260, lw=lw, after=[("label24", None, h.lw("label24"))])
    c.checks(("checkHidden",), F)
    c.field("labelMedLabAcctNum", "textMedLabAcctNum", F, 216, lw=lw)
    left_bottom = c.bottom_used
    # The right column: the address tabs and the billing provider group at the right, the claim defaults under them on their own field column.
    TW = 460
    xt = R - TW
    TH = 158
    h.set("tabControl1", xt, 8, TW, TH)
    PW = TW - 4
    PH = TH - 23
    for page, note, check, rows in (
            ("PhysicalAddress", None, None, ("label3", "textAddress", "label4", "textAddress2", "label11", "textCity", "textState", "textZip")),
            ("BillingAddress", ("label18", 18), "checkUseBillingAddressOnClaims",
             ("label19", "textBillingAddress", "label16", "textBillingAddress2", "label20", "textBillingCity", "textBillingST", "textBillingZip")),
            ("PayToAddress", ("label17", 42), None,
             ("label14", "textPayToAddress", "label13", "textPayToAddress2", "label15", "textPayToCity", "textPayToST", "textPayToZip"))):
        pg = h.n(page)
        if note:
            h.set(note[0], 8, 4, PW - 16, note[1])
        if check:
            h.check_left(check)
            h.set(check, 8, 4 + 18 + 2, h.cw(check), CH)
        la, ta, lb, tb, lc, tcity, tst, tzip = rows
        plw = max(h.lw(x) for x in (la, lb, lc))
        F2 = 8 + plw + 6
        RW = PW - 8 - F2
        y = PH - 8 - 3 * H - 2 * 4
        for lab, fld in ((la, ta), (lb, tb)):
            h.set(lab, F2 - 6 - plw, y, plw, H)
            h.text_align(lab, "MiddleRight")
            h.set(fld, F2, y, RW, H)
            y += H + 4
        h.set(lc, F2 - 6 - plw, y, plw, H)
        h.text_align(lc, "MiddleRight")
        wz, ws = 72, 64
        h.set(tzip, PW - 8 - wz, y, wz, H)
        h.set(tst, PW - 8 - wz - 6 - ws, y, ws, H)
        h.set(tcity, F2, y, PW - 8 - wz - 6 - ws - 6 - F2, H)
        h.set(page, 2, 21, PW, PH)
    h.set("tabSpecialty", 2, 21, PW, PH)
    h.set("gridSpecialty", 8, 8, PW - 8 - 76 - 6 - 8, PH - 16)
    h.set("butAdd", PW - 8 - 76, 8, 76, 24)
    h.set("butRemove", PW - 8 - 76, 8 + 24 + 4, 76, 24)
    gb = h.box("groupBox4", xt, 8 + TH + 8, TW)
    gb.checks(("radioInsBillingProvDefault", "radioInsBillingProvTreat", "radioInsBillingProvSpecific"), gb.left)
    gb.row(("comboInsBillingProv", gb.left, 216), ("butPickInsBillingProv", None, 24))
    gb.end()
    rlabels = ("label7", "label12", "labelDefaultBillingType", "labelInsAutoReceiveNoAssign")
    rlw = max(h.lw(x) for x in rlabels)
    RF = R - 24 - 6 - 48 - 6 - 216
    r = h.box(".")
    r.y = 8 + TH + 8 + h.n("groupBox4").nh + 8
    r.field("label7", "comboPlaceService", RF, 216, lw=rlw)
    r.field("label12", "comboDefaultProvider", RF, 216, lw=rlw, after=[("butPickDefaultProv", None, 24), ("butNone", None, 48)])
    r.field("labelDefaultBillingType", "comboDefaultBillingType", RF, 216, lw=rlw)
    r.field("labelInsAutoReceiveNoAssign", "comboInsAutoReceiveNoAssign", RF, 216, lw=rlw)
    # Its note under the check box's text (after the text it would pass the window's edge).
    r.checks(("checkAlwaysAssignBenToPatient",), RF, gap=0)
    r.row(("label25", RF + 16, h.lw("label25"), 18))
    # The bottom bar 8 px under the taller column.
    content = max(left_bottom, r.bottom_used)
    hw = max(root.node.h, content + 12 + 24 + 8)
    h.set("butSave", R - 75, hw - 8 - 24, 75, 24)
    root.end(w=WW, h=hw)
