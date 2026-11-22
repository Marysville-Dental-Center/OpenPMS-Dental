"""FormInsPlan (Edit Insurance Plan) on the grid (UI-12 round 3, layout script of grid_hand.py). The 26.1 layout: a band of patient plan fields
at the top, three columns under it (the plan and its carrier; other insurance info, write-offs, per visit amounts and ortho; the subscriber,
the benefits grid, the plan note and the Canadian fields), the bottom bar. One field column per column of the window, shared by the group
boxes stacked in it; labels 6 px before their fields; rows 27 px apart; check box lists at the content's left edge; the window grows to
1448 x 845 (the 1920 x 1080 screen holds it). ApplyLayout26_1 moves the plan note, Edit Benefits and the grid's height by the Canadian group's
height outside Canada: relative moves, laid out here with their column."""

W1 = 448   # the plan column (panelPlan)
W2 = 448   # the other insurance info column (panelOrthInfo)
W3 = 544   # the subscriber column, on the window
TOP = 128  # the patient band's height (panelPat), then the 2 px rule (panel1)


def layout(h):
    H = h.H
    pitch = H + 4

    # ---- The patient band (panelPat) -----------------------------------------------------------------------------------------------------
    # The band reaches the request and import groups at the top right (laid out below, right-aligned to the window's content edge).
    wgr = 8 + h.lw("labelHistElect") + 6 + 80 + 8
    wgi = 8 + 3 * 64 + 12 + 8
    wp = (W1 + W2 + 8 + W3) - wgi - 8 - wgr - 8
    p = h.box("panelPat", 0, 0, wp, TOP)
    fa = 8 + max(h.lw("label26"), h.lw("labelPatID")) + 6
    p.row(("label33", 8, h.lw("label33") + 8), ("butAuditPat", fa, None))
    h.text_align("label33", "MiddleLeft")
    p.field("label26", "comboRelationship", fa, 152)
    p.field("labelPatID", "textPatID", fa, 152)
    p.row(("butDrop", 8, None), ("labelDrop", None, h.lw("labelDrop")))
    h.text_align("labelDrop", "MiddleLeft")
    # The plan's own fields: a second band.
    xb0 = fa + 152 + GRID_BAND
    fb = xb0 + max(h.lw("label27"), h.lw("label31"), h.lw("label30")) + 6
    p.y = p.top
    p.field("label27", "textPatPlanNum", fb, 144, after=[("butHistory", None, None)])
    hist_x = h.n("butHistory").nx
    p.field("label31", "textOrdinal", fb, 44, after=[("checkIsPending", None, 16), ("labelPending", None, h.lw("labelPending"))])
    h.set("labelPending", h.n("checkIsPending").nx + 16)
    h.text_align("labelPending", "MiddleLeft")
    h.set("butPatOrtho", hist_x, h.n("textOrdinal").ny, None, H)
    p.field("label30", "textDateLastVerifiedPatPlan", fb, 72, after=[("butVerifyPatPlan", None, None)])
    # Adjustments to insurance benefits: a caption, its Add button at the list's right edge, the list under them.
    xc = max(hist_x + h.n("butHistory").nw, h.n("labelDrop").nx + h.n("labelDrop").nw) + GRID_BAND
    wl = (wp - 8) - xc
    p.y = p.top
    p.row(("label35", xc, wl - h.n("butAdjAdd").nw - 6), ("butAdjAdd", xc + wl - h.n("butAdjAdd").nw, None))
    h.text_align("label35", "MiddleLeft")
    p.place("listAdj", xc, wl, 3 * pitch - 4)
    h.set("panel1", 0, TOP, W1 + W2 + W3 + 16, 2)

    # ---- The plan column (panelPlan) ---------------------------------------------------------------------------------------------------
    y0 = TOP + 2
    c1 = h.box("panelPlan", 0, y0, W1)
    X1 = 168  # the column's fields (panel coordinates): the widest label is "Patient Co-pay Amounts" in a group box
    right1 = W1 - 16  # fields end at the group boxes' content edge
    c1.row(("label32", 8, h.lw("label32") + 8), ("butAudit", right1 - h.n("butPick").nw - 6 - h.n("butAudit").nw, None),
           ("butPick", None, None))
    h.text_align("label32", "MiddleLeft")
    c1.skip(4)
    gp = h.box("groupPlan", 8, c1.y, W1 - 16)
    xg = X1 - 8
    gp.field("label29", "textInsPlanNum", xg, 100, after=[("checkIsMedical", None, 16), ("labelMedicalInsurance", None, h.lw("labelMedicalInsurance"))])
    h.set("checkIsMedical", h.n("textInsPlanNum").nx + 100 + 12)
    h.set("labelMedicalInsurance", h.n("checkIsMedical").nx + 16)
    h.text_align("labelMedicalInsurance", "MiddleLeft")
    gp.field("label16", "textEmployer", xg, "R")
    gc = h.box("groupCarrier", 8, gp.y, gp.node.nw - 16)
    xc_ = xg - 8
    gc.field("label17", "textCarrier", xc_, ("R", H + 6), after=[("butPickCarrier", None, H)])
    gc.field("label7", "textPhone", xc_, 156)
    gc.field("label10", "textAddress", xc_, "R")
    gc.field("label21", "textAddress2", xc_, "R")
    wz = 72
    ws = 60
    wcity = gc.right - xc_ - 12 - ws - wz
    gc.field("labelCitySTZip", "textCity", xc_, wcity, after=[("textState", None, ws), ("textZip", None, wz)])
    ws_ = h.n("butSearch").nw
    gc.field("labelElectronicID", "textElectID", xc_, 56, after=[("comboElectIDdescript", None, gc.right - (xc_ + 56 + 6) - 6 - ws_), ("butSearch", None, ws_)])
    gc.field("labelSendElectronically", "comboSendElectronically", xc_, "R")
    gc.end()
    gp.y = h.n("groupCarrier").ny + h.n("groupCarrier").nh + 8
    gp.bottom_used = gp.y - 8
    wbin = 72
    lbin = max(h.lw("labelBIN"), h.lw("labelDivisionDash"))
    xbin = gp.right - wbin
    gp.field("label8", "textGroupName", xg, xbin - 6 - lbin - 16 - xg, after=[("labelBIN", xbin - 6 - lbin, lbin), ("textBIN", xbin, wbin)])
    gp.field("labelGroupNum", "textGroupNum", xg, xbin - 6 - lbin - 16 - xg,
             after=[("labelDivisionDash", xbin - 6 - lbin, lbin), ("textDivisionNo", xbin, wbin)])
    for lb in ("labelBIN", "labelDivisionDash"):
        h.text_align(lb, "MiddleRight")
    gp.field("label4", "textLinkedNum", xg, 40, after=[("comboLinked", None, "R")])
    # Hidden; the code puts it where comboLinked is.
    h.set("butOtherSubscribers", h.n("comboLinked").nx, h.n("comboLinked").ny, None, H)
    gp.end()
    c1.y = gp.node.ny + gp.node.nh + 8
    c1.bottom_used = c1.y - 8
    c1.field("label14", "comboPlanType", X1, 160, after=[("checkUseBlueBook", None, None)])
    c1.field("label1", "comboFeeSched", X1, right1 - X1)
    c1.skip(4)
    ga = h.box("groupCarrierAllowedAmounts", 8, c1.y, W1 - 16)
    ga.field("labelOutOfNetwork", "comboOutOfNetwork", X1 - 8, "R")
    ga.field("labelManualBlueBook", "comboManualBlueBook", X1 - 8, "R")
    ga.end()
    c1.y = ga.node.ny + ga.node.nh + 8
    gcp = h.box("groupCoPay", 8, c1.y, W1 - 16)
    gcp.row(("label3", 8, "R", h.n("label3").text_h + 2))
    h.text_align("label3", "MiddleLeft")
    gcp.field("labelCopayFeeSched", "comboCopay", X1 - 8, "R")
    gcp.end()
    c1.bottom_used = ga.node.ny + ga.node.nh + 8 + gcp.node.nh

    # ---- The other insurance info column (panelOrthInfo) ---------------------------------------------------------------------------------
    c2 = h.box("panelOrthInfo", W1, y0, W2)
    X2 = 160  # the column's fields: "Age or Frequency Limit" in the write-offs group is its widest label
    right2 = W2 - 16
    c2.row(("labelOtherInsInfo", 8, h.lw("labelOtherInsInfo") + 8))
    h.text_align("labelOtherInsInfo", "MiddleLeft")
    c2.skip(4)
    c2.checks(["checkAlternateCode", "checkCodeSubst", "checkPpoSubWo", "checkClaimsUseUCR", "checkIsHidden", "checkShowBaseUnits"], 8,
              after={"checkAlternateCode": [("label41", None, h.lw("label41"))], "checkCodeSubst": [("label40", None, h.lw("label40"))],
                     "checkPpoSubWo": [("butSubstCodes", None, None, H)], "checkShowBaseUnits": [("label12", None, h.lw("label12"))]})
    for lb in ("label41", "label40", "label12"):
        h.text_align(lb, "MiddleLeft")
    c2.skip(8)
    c2.field("label23", "comboClaimForm", X2, right2 - X2)
    c2.field("label20", "comboCobRule", X2, right2 - X2)
    c2.field("label13", "comboFilingCode", X2, right2 - X2)
    c2.field("label15", "comboFilingCodeSubtype", X2, right2 - X2)
    c2.field("label38", "comboBillType", X2, right2 - X2)
    c2.skip(4)
    go = h.box("groupBoxOD1", 8, c2.y, W2 - 16)
    go.field("label11", "comboExclusionFeeRule", X2 - 8, "R")
    go.field("labelZeroWriteoffAnnualMax", "comboBoxZeroWriteOffsOnAnnualMaxLimitation", X2 - 8, "R")
    go.field("label39", "comboZeroWriteOffsOnLimitationOrAging", X2 - 8, "R")
    go.end()
    gv = h.box("groupBoxPerVisitAmounts", 8, go.node.ny + go.node.nh + 8, W2 - 16)
    xv = X2 - 8
    gv.field("labelPerVisitPatAmount", "textPerVisitPatAmount", xv, 72)
    gv.field("labelPerVisitInsuranceAmount", "textPerVisitInsAmount", xv, 72)
    xn = xv + 72 + 16
    h.set("labelPerVisit", xn, gv.top, gv.right - xn, 2 * H + 4)
    h.text_align("labelPerVisit", "MiddleLeft")
    gv.end()
    gor = h.box("groupOrtho", 8, gv.node.ny + gv.node.nh + 8, W2 - 16)
    po = h.box("panelOrtho", 8, gor.top, gor.node.nw - 16, None, pad=0)
    xo = X2 - 16
    po.field("label36", "comboOrthoClaimType", xo, "R")
    po.field("label37", "textOrthoAutoProc", xo, 120, after=[("butPickOrthoProc", None, H), ("butDefaultAutoOrthoProc", None, None)])
    po.field("labelOrthoAutoFee", "textOrthoAutoFee", xo, 80)
    po.field("labelAutoOrthoProcPeriod", "comboOrthoAutoProcPeriod", xo, 160)
    # Its text does not fit at the field column: a check box alone under the fields starts at the content's left edge.
    po.row(("checkOrthoWaitDays", 0, None))
    po.end()
    gor.y = po.node.ny + po.node.nh
    gor.bottom_used = gor.y
    gor.end()
    c2.bottom_used = gor.node.ny + gor.node.nh

    # ---- The subscriber column (on the window) -----------------------------------------------------------------------------------------
    x3 = W1 + W2 + 8
    gs = h.box("groupSubscriber", x3, y0 + 8, W3)
    xs = 8 + max(h.lw(n) for n in ("label25", "label2", "label5", "label28")) + 6
    wch = h.n("butChange").nw
    gs.field("label25", "textSubscriber", xs, ("R", wch + 6), after=[("butChange", None, wch)])
    wdate = 80
    lto = h.lw("label6")
    xchk = xs + wdate + 6 + lto + 6 + wdate + 16
    gs.field("label2", "textSubscriberID", xs, 2 * wdate + 12 + lto, after=[("checkRelease", xchk, 16), ("labelRelease", xchk + 16, h.lw("labelRelease"))])
    gs.field("label5", "textDateEffect", xs, wdate, after=[("label6", None, lto), ("textDateTerm", None, wdate), ("checkAssign", xchk, 16),
                                                          ("labelAssign", xchk + 16, h.lw("labelAssign"))])
    for lb in ("labelRelease", "labelAssign"):
        h.text_align(lb, "MiddleLeft")
    h.text_align("label6", "MiddleCenter")
    gs.row(("label28", xs - 6 - h.lw("label28"), h.lw("label28"), H), ("textSubscNote", xs, "R", 72))
    h.text_align("label28", "TopRight")
    h.set("label28", None, h.n("textSubscNote").ny + 3)
    gs.end()
    # The benefits grid, the plan note under it, the Canadian group at the bottom of the column (outside Canada the note moves down into
    # its space and the grid grows).
    gcan = h.box("groupCanadian", x3, 0, W3)
    xk = 8 + max(h.lw("label22"), h.lw("label9")) + 6
    gcan.field("label22", "textPlanFlag", xk, 40, after=[("label24", None, h.lw("label24"))])
    h.text_align("label24", "MiddleLeft")
    lin = h.lw("label19")
    xin = gcan.right - 96
    h.set("checkIsPMP", xin - 6 - lin, gcan.top + (H - h.CH) // 2, h.cw("checkIsPMP"), h.CH)
    h.check_left("checkIsPMP")
    gcan.field("label9", "textCanadianDiagCode", xk, 96, after=[("label19", xin - 6 - lin, lin), ("textCanadianInstCode", xin, 96)])
    h.text_align("label19", "MiddleRight")
    gcan.end()
    bottom3 = max(c1.node.ny + c1.bottom_used, c2.node.ny + c2.bottom_used)  # the columns end together
    hcan = gcan.node.nh
    ycan = bottom3 - hcan
    h.set("groupCanadian", x3, ycan)
    ynote = ycan - 8 - 72
    h.set("textPlanNote", x3, ynote, W3, 72)
    ycap = ynote - 4 - H
    h.set("label18", x3, ycap, h.lw("label18"), H)
    h.text_align("label18", "MiddleLeft")
    h.set("butBenefits", x3 + W3 - h.n("butBenefits").nw, ycap, None, H)
    ygrid = gs.node.ny + gs.node.nh + 8
    h.set("gridBenefits", x3, ygrid, W3, ycap - 8 - ygrid)
    for c in (c1, c2):
        c.end(h=bottom3 + 8 - c.node.ny)

    # ---- The top right: requests, imports, benefits verified (right-aligned to the window's content edge) -------------------------------
    xr = x3 + W3
    gi = h.box("groupBox1", 0, 8, None)
    bw = 64
    gi.row(("butImportTrojan", 8, bw), ("butIapFind", None, bw), ("butBenefitNotes", None, bw))
    gi.field("labelTrojanID", "textTrojanID", 8 + h.lw("labelTrojanID") + 6, 8 + 3 * bw + 12 - (8 + h.lw("labelTrojanID") + 6))
    wgi = 8 + 3 * bw + 12 + 8
    gi.end(w=wgi)
    h.set("groupBox1", xr - wgi, 8)
    gr = h.box("groupRequestBen", 0, 8, None)
    lr = h.lw("labelHistElect")
    xrq = 8 + lr + 6
    gr.field("labelHistElect", "textElectBenLastDate", xrq, 80)
    bw2 = (80 + lr + 6 - 6) // 2
    gr.row(("butGetElectronic", 8, bw2), ("butHistoryElect", None, bw2))
    wgr = 8 + lr + 6 + 80 + 8
    gr.end(w=wgr)
    h.set("groupRequestBen", xr - wgi - 8 - wgr, 8)
    yv = 8 + max(gi.node.nh, gr.node.nh) + 8
    wdv = h.cw("checkDontVerify")
    h.check_left("checkDontVerify")
    xdv = xr - wdv
    xnow = xdv - 16 - h.n("butVerifyBenefits").nw
    xd = xnow - 6 - 80
    lv = h.lw("label34")
    h.set("label34", xd - 6 - lv, yv, lv, H)
    h.text_align("label34", "MiddleRight")
    h.set("textDateLastVerifiedBenefits", xd, yv, 80, H)
    h.set("butVerifyBenefits", xnow, yv, None, H)
    h.set("checkDontVerify", xdv, yv + (H - h.CH) // 2, wdv, h.CH)

    # ---- The bottom bar ----------------------------------------------------------------------------------------------------------------
    wwin = x3 + W3 + 8
    ybar = bottom3 + 8 + 4
    hb = h.n("butSave").nh
    h.set("butDelete", 8, ybar)
    h.set("butLabel", 8 + h.n("butDelete").nw + 8, ybar)
    h.set("butSave", wwin - 8 - h.n("butSave").nw, ybar)
    rg = h.box("groupChanges", 0, 0, None, None, pad=0)
    wr1 = h.cw("radioCreateNew")
    wr2 = h.cw("radioChangeAll")
    rg.row(("radioCreateNew", 8, wr1), ("radioChangeAll", 8 + wr1 + 16, wr2), h=h.CH)
    wrg = 8 + wr1 + 16 + wr2 + 8
    rg.end(w=wrg, h=h.CH + 6)
    h.set("radioCreateNew", None, 3)
    h.set("radioChangeAll", None, 3)
    h.set("groupChanges", wwin - 8 - h.n("butSave").nw - 8 - wrg, ybar + (hb - h.CH - 6) // 2)
    h.set(".", None, None, wwin, ybar + hb + 8)


GRID_BAND = 16
