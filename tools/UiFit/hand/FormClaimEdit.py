"""FormClaimEdit (Edit Claim) on the grid (UI-12 round 3, layout script of grid_hand.py). Its General, Misc and Attachments tabs are laid out by
the planner in parts (grid-scope.tsv); this script runs on the planner's tree and places the rest: two bands of claim fields (one field column
each), the insurance plan and other coverage groups stacked beside them, the payment groups side by side with one height, the procedures grid,
the totals under its columns, the payments grid with the finalize payment group and the reasons underpaid, the tab control and the bottom bar.
Everything ends 8 px from the window's edges; the window grows to 1168 x 954 (SetBounds makes it at least as tall as the screen allows up to
871 px, so the designer's height now decides). The Enter Payment and Pending Payment groups and their buttons stand where the designer has
them: the code shows one or the other."""


def layout(h):
    H = h.H
    WW = 1168
    right = WW - 8
    root = h.box(".", None, None, None, None)
    # ---- Band 1: status, type and the dates --------------------------------------------------------------------------------------------
    h.set("warningIntegrity", 8, 8 + (H - 18) // 2)
    F1 = 112
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
    w3 = 280
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
    wbtn = 104
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
