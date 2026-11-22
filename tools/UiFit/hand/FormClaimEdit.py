"""FormClaimEdit (Edit Claim) on the grid (UI-12 round 3, layout script of grid_hand.py). Its General, Misc and Attachments tabs are laid out by
the planner in parts (grid-scope.tsv); this script runs on the planner's tree and places the rest: two bands of claim fields (one field column
each), the insurance plan and other coverage groups stacked beside them, the payment groups side by side with one height, the procedures grid,
the totals under its columns, the payments grid with the finalize payment group and the reasons underpaid, the tab control and the bottom bar.
Everything ends 8 px from the window's edges; the window grows to 1168 x 954 (SetBounds makes it at least as tall as the screen allows up to
871 px, so the designer's height now decides). The Enter Payment and Pending Payment groups and their buttons stand where the designer has
them: the code shows one or the other. The UB04 and Canadian tabs are laid out here too (the planner does not settle their nested groups)."""


def layout(h):
    H = h.H
    WW = 1168
    right = WW - 8
    root = h.box(".", None, None, None, None)
    # ---- Band 1: status, type and the dates --------------------------------------------------------------------------------------------
    h.set("warningIntegrity", 8, 8 + (H - 18) // 2)
    F1 = 116  # on the UB04 tab's date groups' field column (window coordinates)
    root.y = 8
    root.field("label2", "comboClaimStatus", F1, 150)
    root.field("label9", "comboClaimType", F1, 150)
    root.field("labelDateService", "textDateService", F1, 88)
    root.field("label8", "textDateSentOrig", F1, 88)
    root.field("labelDateSent", "textDateSent", F1, 88, after=[("butResend", None, None)])
    root.field("label6", "textDateRec", F1, 88)
    bottom1 = root.y - 4
    # ---- Band 2: clinic, medical/dental, claim form, providers, predetermination ---------------------------------------------------------
    x2 = F1 + 150 + 16
    F2 = x2 + max(h.lw(n) for n in ("label86", "label87", "label3", "label21", "labelPredeterm")) + 6
    root.y = 8
    h.set("comboClinic", F2 - 37, root.y, 37 + 150, H)
    root.y += H + 4
    root.field("label86", "comboMedType", F2, 150)
    root.field("label87", "comboClaimForm", F2, 150)
    root.field("label3", "comboProvBill", F2, 150, after=[("butPickProvBill", None, H)])
    root.field("label21", "comboProvTreat", F2, 150, after=[("butPickProvTreat", None, H)])
    root.field("labelPredeterm", "textPredeterm", F2, 150 + 6 + H)
    right2 = F2 + 150 + 6 + H
    # ---- Band 3: the insurance plan and the other coverage --------------------------------------------------------------------------
    x3 = right2 + 16
    # Polish round: the three columns of the top right (the plan groups, Enter Payment, Pending Payment) end on the procedures grid's right
    # edge, the room past their old widths (280, 120, 120) shared evenly between them.
    extra = right - (x3 + 280 + 8 + 2 * 120 + 8)
    e_pay = (extra + 2) // 3
    w3 = 280 + extra - 2 * e_pay
    g2 = h.box("groupBox2", x3, 8, w3)
    g2.row(("textPlan", 8, "R"))
    fr = 8 + max(h.lw("label5"), h.lw("label10")) + 6
    g2.field("label5", "comboPatRelat", fr, "R")
    g2.end()
    g3 = h.box("groupBox3", x3, 8 + g2.node.nh + 8, w3)
    wb = h.n("butOtherCovChange").nw
    g3.row(("textPlan2", 8, ("R", 2 * (wb + 6))), ("butOtherCovChange", None, wb), ("butOtherNone", None, wb))
    g3.field("label10", "comboPatRelat2", fr, "R")
    g3.end()
    bottom3 = g3.node.ny + g3.node.nh
    # ---- Band 4: entering payments (Split Claim under them) ------------------------------------------------------------------------------
    x4 = x3 + w3 + 8
    wbtn = 104 + e_pay
    ge = h.box("groupEnterPayment", x4, 8, wbtn + 16)
    for name in ("butPayTotal", "butPayProc", "butPaySupp"):
        ge.row((name, 8, wbtn))
    ge.end()
    gpp = h.box("groupBoxPendingPayment", x4 + wbtn + 16 + 8, 8, wbtn + 16)
    for name in ("butInsuranceOverpaid", "butInsuranceUnderpaid"):
        gpp.row((name, 8, wbtn))
    gpp.end(h=ge.node.nh)
    h.set("butSplit", x4 + 8, ge.node.ny + ge.node.nh + 8, wbtn, H)
    top_bottom = max(bottom1, root.y - 4, bottom3, h.n("butSplit").ny + H)
    # ---- The procedures grid and the totals under its columns -----------------------------------------------------------------------------
    gp = h.n("gridProc")
    dx = 8 - gp.nx
    yg = top_bottom + 8
    h.set("gridProc", 8, yg, right - 8, gp.nh)
    yt = yg + gp.nh + 4
    h.set("butViewEra", 8, yt, None, H)
    h.set("butViewEob", 8 + h.n("butViewEra").nw + 6, yt, None, H)
    for name in ("textClaimFee", "textDedApplied", "textInsPayEst", "textInsPayAmt", "textWriteOff", "textPatResp"):
        n = h.n(name)
        h.set(name, n.nx + dx, yt, n.nw, H, why="the totals under the grid's columns (they keep their x with the grid)")
    lt = h.lw("label1")
    h.set("label1", h.n("textClaimFee").nx - 6 - lt, yt, lt, H)
    h.text_align("label1", "MiddleRight")
    last = h.n("textPatResp")
    h.set("butRecalc", last.nx + last.nw + 16, yt, None, H)
    # ---- The payments grid, finalizing payments, the reasons underpaid -----------------------------------------------------------------
    yp = yt + H + 8
    gf = h.box("groupFinalizePayment", 0, yp, 184)
    gf.row(("butBatch", 8, 104))
    gf.row(("labelBatch", 8, "R", 2 * 17 + 2))
    h.text_align("labelBatch", "MiddleLeft")
    gf.row(("butThisClaimOnly", 8, 104))
    gf.end()
    hp = gf.node.nh
    wpay = h.n("gridPay").nw
    h.set("gridPay", 8, yp, wpay, hp)
    h.set("groupFinalizePayment", 8 + wpay + 8, yp)
    xr = 8 + wpay + 8 + 184 + 8
    h.set("label4", xr, yp, right - xr, 17)
    h.text_align("label4", "BottomLeft")
    h.set("textReasonUnder", xr, yp + 17 + 2, right - xr, hp - 17 - 2)
    # ---- The tab control and the bottom bar ----------------------------------------------------------------------------------------------
    tc = h.n("tabControlMain")
    ytab = yp + hp + 8
    h.set("tabControlMain", 8, ytab, right - 8, tc.nh)
    # The General tab's claim note ends 8 px from the tab's edge (the planner keeps a note box's width; its panel keeps the tab's).
    pn = h.n("panel2")
    for name in ("labelNote", "textNote"):
        n = h.n(name)
        h.set(name, None, None, pn.nw - 8 - n.nx)
    ybar = ytab + tc.nh + 12
    x = 8
    h.set("butDelete", x, ybar, None, 24)
    x += h.n("butDelete").nw + 24
    for name in ("butLabel", "butPreview", "butPrint", "butSend", "butHistory"):
        h.set(name, x, ybar, None, 24)
        x += h.n(name).nw + 6
    ws = h.n("butSave").nw
    h.set("butSave", right - ws, ybar, None, 24)
    w20 = 168
    h.set("label20", right - ws - 8 - w20, ybar + 24 - 34, w20, 34)
    h.text_align("label20", "BottomRight")
    h.set(".", None, None, WW, ybar + 24 + 8)
    layout_ub04(h)
    layout_canadian(h)


def layout_ub04(h):
    """The UB04 tab (its panel scrolls): the ordering provider and the two date groups in the left column, the UB04 group beside them with its
    four codes in a field column and the condition codes and value codes groups beside them (one box per code, captions centred over them)."""
    H = h.H
    p = h.n("panel4")
    wl = 322
    g4 = h.box("groupBox4", 8, 8, wl)
    g4.row(("butPickOrderProvInternal", 8, 64), ("butPickOrderProvReferral", None, 64), ("butNoneOrderProv", None, 64))
    g4.row(("textOrderingProviderOverride", 8, "R"))
    g4.end()
    y = 8 + g4.node.nh + 8
    for gname, ld, td, lq, cq in (("groupDateIllnessInjuryPreg", "labelDateIllness", "textDateIllness", "labelDateIllnessQualifier", "comboDateIllnessQualifier"),
                                  ("groupDateOtherCondOrTreatment", "labelDateOther", "textDateOther", "labelDateOtherQualifier", "comboDateOtherQualifier")):
        g = h.box(gname, 8, y, wl)
        f = 8 + max(h.lw(ld), h.lw(lq)) + 6
        g.field(ld, td, f, 80)
        g.field(lq, cq, f, "R")
        g.end()
        y += g.node.nh + 8
    h.set("checkIsOutsideLab", 8, y, h.cw("checkIsOutsideLab"), h.CH)
    h.check_left("checkIsOutsideLab")
    left_bottom = y + h.CH
    # The UB04 group.
    xu = 8 + wl + 8
    gu = h.box("groupUb04", xu, 8, None)
    # On the General tab's ortho and referral field column (530 px in the window): the widest label needs 166.
    fu = max(8 + max(h.lw(n) for n in ("labelBillType", "labelAdmissionType", "labelAdmissionSource", "labelPatientStatus")) + 6, 182)
    for lb, tb in (("labelBillType", "textBillType"), ("labelAdmissionType", "textAdmissionType"), ("labelAdmissionSource", "textAdmissionSource"),
                   ("labelPatientStatus", "textPatientStatus")):
        gu.field(lb, tb, fu, 48)
    xg = fu + 48 + 16
    wbox, gap = 26, 6
    # Value codes: three sets (39, 40, 41), each a letter, a code box and an amount box, under a header row.
    wlet = 24
    wset = wlet + 6 + wbox + 6 + 66
    wg = 8 + 3 * wset + 2 * 16 + 8
    gc = h.box("groupBox1", xg, gu.top, wg)
    for i in range(11):
        x = 8 + i * (wbox + gap)
        h.set("labelCode%d" % i, x - 3, gc.top, wbox + 6, 17)
        h.text_align("labelCode%d" % i, "BottomCenter")
        h.set("textCode%d" % i, x, gc.top + 17 + 2, wbox, H)
    gc.end(h=gc.top + 17 + 2 + H + 8)
    gv = h.box("groupValueCodes", xg, gu.top + gc.node.nh + 8, wg)
    for s, code in enumerate(("39", "40", "41")):
        x0 = 8 + s * (wset + 16)
        xc = x0 + wlet + 6
        xa = xc + wbox + 6
        h.set("labelVC" + code, x0, gv.top, wlet, 17)
        h.set("labelVC%sCode" % code, xc - 6, gv.top, wbox + 12, 17)
        h.set("labelVC%sAmt" % code, xa, gv.top, 66, 17)
        for lb in ("labelVC" + code, "labelVC%sCode" % code, "labelVC%sAmt" % code):
            h.text_align(lb, "BottomCenter")
        for r, letter in enumerate("abcd"):
            yr = gv.top + 17 + 2 + r * (H + 4)
            h.set("labelVC%s%s" % (code, letter), x0, yr, wlet, H)
            h.text_align("labelVC%s%s" % (code, letter), "MiddleCenter")
            h.set("textVC%s%sCode" % (code, letter), xc, yr, wbox, H)
            h.set("textVC%s%sAmt" % (code, letter), xa, yr, 66, H)
    gv.end(h=gv.top + 17 + 2 + 4 * (H + 4) - 4 + 8)
    gu.bottom_used = max(gu.y - 4, gv.node.ny + gv.node.nh)
    gu.end(w=xg + wg + 8)
    h.set("panel4", None, None, None, max(339, left_bottom + 8, 8 + gu.node.nh + 8))


def layout_canadian(h):
    """The Canadian tab (its panel scrolls): the referring provider, the accident and transaction rows and the ortho predetermination group in
    the first column, the two prosthesis groups stacked beside it, the extracted and missing teeth and the materials forwarded in the last."""
    H = h.H
    w1 = 386
    g9 = h.box("groupBox9", 8, 8, w1)
    # On the General and Misc tabs' field column (186 px in the window): the widest label needs 158.
    f9 = max(8 + max(h.lw("label73"), h.lw("label72")) + 6, 168)
    g9.field("label73", "textReferralProvider", f9, 100)
    g9.field("label72", "comboReferralReason", f9, "R")
    g9.end()
    fp = 8 + f9  # the panel's field column: the group's, in the panel
    root = h.box("panel5", None, None, None)
    root.y = 8 + g9.node.nh + 8
    root.field("label43", "textCanadianAccidentDate", fp, 80)
    root.field("label76", "textCanadaTransRefNum", fp, 100, after=[("butReverse", None, None)])
    root.row(("checkCanadianIsOrtho", fp, None))
    rows_bottom = root.y - 4
    yo = rows_bottom + 8
    go = h.box("groupCanadaOrthoPredeterm", 8, yo, w1)
    # More than 40 px right of the tabs' field column (186 px in the window), which the widest label (188 px) cannot reach.
    fo = max(8 + max(h.lw(n) for n in ("label77", "label78", "label79", "label80", "label81", "label82")) + 6, 212)
    for la, ta in (("label77", "textDateCanadaEstTreatStartDate"), ("label80", "textCanadaTreatDuration"), ("label78", "textCanadaInitialPayment"),
                   ("label81", "textCanadaNumPaymentsAnticipated"), ("label79", "textCanadaExpectedPayCycle"), ("label82", "textCanadaAnticipatedPayAmount")):
        go.field(la, ta, fo, 80)
    go.end()
    # The prosthesis groups, stacked beside the first column.
    x3 = 8 + w1 + 8
    w3 = 355
    # More than 40 px right of the General and UB04 tabs' second field column (530 px in the window).
    fm = max(8 + max(h.lw(n) for n in ("label69", "label70", "label71", "label66", "label67", "label68")) + 6, 160)
    y = 8
    for gname, l1, c1, l2, d2, l3, c3 in (("groupMaxPros", "label69", "comboMaxProsth", "label70", "textDateInitialUpper", "label71", "comboMaxProsthMaterial"),
                                          ("groupMandPros", "label66", "comboMandProsth", "label67", "textDateInitialLower", "label68", "comboMandProsthMaterial")):
        g = h.box(gname, x3, y, w3)
        g.field(l1, c1, fm, "R")
        g.field(l2, d2, fm, 80)
        g.field(l3, c3, fm, "R")
        g.end()
        y += g.node.nh + 8
    # The teeth.
    x4 = x3 + w3 + 8
    w4 = 172
    wq = h.n("butMissingTeethHelp").nw
    h.set("labelExtractedTeeth", x4, 8, w4 - wq - 6, H)
    h.text_align("labelExtractedTeeth", "BottomLeft")
    h.set("butMissingTeethHelp", x4 + w4 - wq, 8, wq, H)
    h.set("listExtractedTeeth", x4, 8 + H + 2, w4, 95)
    ym = 8 + H + 2 + 95 + 8
    h.set("labelCanadaMissingTeeth", x4, ym, w4, 17)
    h.text_align("labelCanadaMissingTeeth", "BottomLeft")
    h.set("textMissingTeeth", x4, ym + 17 + 2, w4, 44)
    # The materials forwarded under the teeth.
    wm = w4
    gm = h.box("groupBox8", x4, ym + 17 + 2 + 44 + 8, wm)
    gm.checks(["checkEmail", "checkCorrespondence", "checkModels", "checkXrays", "checkImages"], 8)
    gm.end()
    h.set("panel5", None, None, None, max(339, go.node.ny + go.node.nh + 8, gm.node.ny + gm.node.nh + 8))
