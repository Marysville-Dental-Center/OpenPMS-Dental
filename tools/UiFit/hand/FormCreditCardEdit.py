"""FormCreditCardEdit on the grid (UI-12 slice 3, mode hand). Main's arrangement on the grid's numbers, one field column for the window and
its groups: Card Number, Exp (MMYY), Address (to the right edge) and Zip; Clinic, Account Type and Card Holder (shown by the code) on a right
field column on the same rows. The Authorized Recurring Charges group: Payment Plan, Payment Type Override, Charge Amount with Clear, Date
Start with Today, Date Stop, Run charge even if no family balance present and Note on the column; Is Recurring Active, Next Charge
Date and Previous Start Date (shown by the code) in a right column. The Charge Frequency group: the frequency in words, the two kinds of
frequency side by side with their boxes under them, Add Day and Clear under the day box. The Authorized Procedures group (shown by the code):
the note before the list, Exclude from proc sync, Add and Remove after it. Delete and Save on the bottom bar. FormCreditCardEdit.cs hides
the recurring part and shortens the window by the band from the recurring group's top to the bottom bar (no longer by a literal 486)."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W = root.w
    R = W - PAD
    H = h.H
    CH = h.CH
    P = H + 4
    inner = ["labelPayPlan", "label9", "label5", "label2", "label1", "label7"]
    ilw = max(h.lw(n) for n in inner)
    Fi = PAD + ilw + 6
    F = PAD + Fi
    olw = F - 6 - PAD
    RF = R - 136
    rows = (("label3", "textCardNumber", 136, "labelClinic", "textClinic"), ("label4", "textExpDate", 71, "labelAcctType", "textAccountType"),
            ("labelAddress", "textAddress", R - F, None, None), ("labelZip", "textZip", 136, "labelCardHolderName", "textCardHolderName"))
    rlw = max(h.lw("labelClinic"), h.lw("labelAcctType"), h.lw("labelCardHolderName"))
    y = PAD
    for lab, fld, w, rl, rf in rows:
        h.set(lab, PAD, y, olw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F, y, w, H)
        if rl:
            h.set(rl, RF - 6 - rlw, y, rlw, H)
            h.text_align(rl, "MiddleRight")
            h.set(rf, RF, y, 136, H)
        y += P
    y += GAP - 4
    gw = R - PAD
    cr = gw - PAD  # the groups' content right edge
    # Authorized Recurring Charges.
    g = h.n("groupRecurringCharges")
    top = (g.title_bottom or 18) + 4
    rfi = cr - 82
    rli = max(h.lw("labelNextChargeDate"), h.lw("labelPreviousStartDate"))
    RC = rfi - 6 - rli  # the right column's start
    cbw = RC - 16 - Fi
    yy = top
    h.set("labelPayPlan", PAD, yy, ilw, H)
    h.set("comboPaymentPlans", Fi, yy, cbw, H)
    yy += P
    h.set("label9", PAD, yy, ilw, H)
    h.set("comboPaymentType", Fi, yy, cbw, H)
    h.check_left("checkIsRecurringActive")
    h.set("checkIsRecurringActive", RC, yy + (H - CH) // 2, h.cw("checkIsRecurringActive"), CH)
    yy += P
    bw = 56
    for lab, fld, but, rl, rf in (("label5", "textChargeAmt", "butClear", "labelNextChargeDate", "textNextChargeDate"),
                                  ("label2", "textDateStart", "butToday", "labelPreviousStartDate", "textPreviousStartDate"),
                                  ("label1", "textDateStop", None, None, None)):
        h.set(lab, PAD, yy, ilw, H)
        h.set(fld, Fi, yy, 88, H)
        if but:
            h.set(but, Fi + 88 + 6, yy, bw, H)
        if rl:
            h.set(rl, rfi - 6 - rli, yy, rli, H)
            h.text_align(rl, "MiddleRight")
            h.set(rf, rfi, yy, 82, H)
        yy += P
    h.check_left("checkChrgWithNoBal")
    h.set("checkChrgWithNoBal", Fi, yy, h.cw("checkChrgWithNoBal"), CH)
    yy += CH + 4
    h.set("label7", PAD, yy, ilw, H)
    h.text_align("label7", "MiddleRight")
    h.set("textNote", Fi, yy, cr - Fi, h.n("textNote").nh)
    for lab in ("labelPayPlan", "label9", "label5", "label2", "label1"):
        h.text_align(lab, "MiddleRight")
    gh = yy + h.n("textNote").nh + PAD
    gy = y
    h.set("groupRecurringCharges", PAD, gy, gw, gh)
    y = gy + gh + GAP
    # Charge Frequency.
    gf = h.n("groupChargeFrequency")
    tf = (gf.title_bottom or 18) + 4
    h.set("labelFrequencyInWords", PAD, tf, cr - PAD, 17)
    h.text_align("labelFrequencyInWords", "MiddleCenter")
    ry = tf + 17 + 4
    C2 = 200
    for r, x in (("radioDayOfMonth", PAD), ("radioWeekDay", C2)):
        h.check_left(r)
        h.set(r, x, ry, h.cw(r), CH)
    fy = ry + CH + 4
    h.set("textDayOfMonth", PAD, fy, 125, H)
    h.set("comboFrequency", C2, fy, 93, H)
    h.set("comboDays", C2 + 93 + 6, fy, 93, H)
    h.set("label8", C2 + 2 * 93 + 12, fy, h.lw("label8"), H)
    h.text_align("label8", "MiddleLeft")
    by_ = fy + P
    h.set("butAddDay", PAD, by_, 60, 24)
    h.set("butClearDayOfMonth", PAD + 66, by_, 59, 24)
    fh = by_ + 24 + PAD
    h.set("groupChargeFrequency", PAD, y, gw, fh)
    y += fh + GAP
    # Authorized Procedures (shown by the code).
    gp = h.n("groupProcedures")
    tp = (gp.title_bottom or 18) + 4
    lp = h.n("listProcs")
    nh = h.note_h("label15", ilw)
    h.set("label15", PAD, tp, ilw, nh)
    h.text_align("label15", "TopRight")
    h.set("listProcs", Fi, tp, lp.nw, lp.nh)
    bx = Fi + lp.nw + GAP
    ex_h = max(CH, h.note_h("checkExcludeProcSync", cr - bx - 16))
    h.check_left("checkExcludeProcSync")
    h.set("checkExcludeProcSync", bx, tp, cr - bx, ex_h)
    h.set("butAddProc", bx, tp + ex_h + 4, 78, 24)
    h.set("butRemoveProc", bx, tp + ex_h + 4 + 28, 78, 24)
    ph = tp + max(lp.nh, nh, ex_h + 4 + 52) + PAD
    h.set("groupProcedures", PAD, y, gw, ph)
    y += ph + GAP
    HH = max(root.h, y + 24 + PAD)
    h.set("butDelete", PAD, HH - PAD - 24, None, 24)
    h.set("butSave", R - h.n("butSave").nw, HH - PAD - 24, None, 24)
    h.set(".", None, None, W, HH)
    h.watch(".", "groupRecurringCharges", "groupChargeFrequency", "groupProcedures")
