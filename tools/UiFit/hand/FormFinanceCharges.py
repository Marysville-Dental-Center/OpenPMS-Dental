"""FormFinanceCharges on the grid (UI-12 slice 2, mode hand: the planner found no fixed point and spread the charge rows across a 1830 px
window). As main arranges it, on the grid: Date last run and Date of new charges on one field column; the charge panel under them (its rows
on the 27 px pitch: Billing Charge and Finance Charge as radio buttons at the content edge with their $ and APR labels 6 px before the same
field column, % and the example 6 px after, the amount rows' labels 16 px in, Compound interest's box on the column); the billing types
caption and list at the right content edge (the list as wide as its caption) down to the panel's bottom; under them the balances aged and
undo groups (174 px) on the left, the filters group 8 px after them to the content edge (mostly check boxes: the Preferences style, its
field column at half its content, the long labels on two lines), the assign group 8 px under them at the content edge (its specific
provider box 6 px after its radio button), Run at the right content edge on its bottom. The window grows by what the rows need."""


def layout(h):
    H = h.H
    CH = h.CH
    P = H + 4
    root = h.tree
    W = root.w
    R = W - 8
    lw1 = max(h.lw("label5"), h.lw("label1"))
    F = max(8 + lw1 + 6, 8 + h.cw("radioFinanceCharge") + 8 + h.lw("label2") + 6, 24 + h.lw("labelAtLeast") + 6 + h.lw("label12") + 6)
    for lab, fld, y in (("label5", "textDateLastRun", 8), ("label1", "textDate", 8 + P)):
        h.set(lab, F - 6 - lw1, y, lw1, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F, y, 78, H)
    yp = 8 + 2 * P
    # The charge panel at x 0, so its coordinates are the window's.
    y = 0
    h.check_left("radioBillingCharge")
    h.set("radioBillingCharge", 8, y + (H - CH) // 2, h.cw("radioBillingCharge"), CH)
    lwd = h.lw("label8")
    h.set("label8", F - 6 - lwd, y, lwd, H)
    h.text_align("label8", "MiddleRight")
    h.set("textBillingCharge", F, y, 42, H)
    y += P
    h.check_left("radioFinanceCharge")
    h.set("radioFinanceCharge", 8, y + (H - CH) // 2, h.cw("radioFinanceCharge"), CH)
    lwa = h.lw("label2")
    h.set("label2", F - 6 - lwa, y, lwa, H)
    h.text_align("label2", "MiddleRight")
    h.set("textAPR", F, y, 42, H)
    h.set("label3", F + 42 + 6, y, h.lw("label3"), H)
    h.text_align("label3", "MiddleLeft")
    h.set("label4", F + 42 + 6 + h.lw("label3") + 6, y, h.lw("label4"), H)
    h.text_align("label4", "MiddleLeft")
    y += P
    for lab, dol, fld in (("labelAtLeast", "label12", "textAtLeast"), ("labelOver", "label11", "textOver")):
        h.set(lab, 24, y, h.lw(lab), H)
        h.text_align(lab, "MiddleLeft")
        lwd = h.lw(dol)
        h.set(dol, F - 6 - lwd, y, lwd, H)
        h.text_align(dol, "MiddleRight")
        h.set(fld, F, y, 42, H)
        y += P
    h.set("labelCompound", 24, y, h.lw("labelCompound"), H)
    h.text_align("labelCompound", "MiddleLeft")
    h.set("checkCompound", F, y + (H - 14) // 2, 16, 14)
    ph = y + H
    pw = F + 42 + 6 + h.lw("label3") + 6 + h.lw("label4") + 8
    h.set("panel1", 0, yp, pw, ph)
    # Billing types
    LW = max(158, h.lw("label7"))
    h.set("label7", R - LW, 8, LW, 18)
    h.text_align("label7", "BottomLeft")
    h.set("listBillType", R - LW, 8 + 18 + 2, LW, yp + ph - (8 + 18 + 2))
    # Groups
    yA = yp + ph + 8
    GL = 174
    t = (h.n("groupBox1").title_bottom or 18) + 4
    y = t
    for r in ("radio30", "radio60", "radio90"):
        h.check_left(r)
        h.set(r, 8, y, h.cw(r), CH)
        y += CH + 4
    g1 = y - 4 + 8
    h.set("groupBox1", 8, yA, GL, g1)
    t = (h.n("groupBox2").title_bottom or 18) + 4
    lw6 = h.lw("label6")
    Fu = 8 + lw6 + 6
    h.set("label6", 8, t, lw6, H)
    h.text_align("label6", "MiddleRight")
    h.set("textDateUndo", Fu, t, GL - 8 - Fu, H)
    h.set("butUndo", Fu, t + P, GL - 8 - Fu, 24)
    g2 = t + P + 24 + 8
    h.set("groupBox2", 8, yA + g1 + 8, GL, g2)
    # Filters (Preferences style)
    fx = 8 + GL + 8
    fw = R - fx
    Ff = 8 + (fw - 16) // 2
    t = (h.n("groupBoxFilters").title_bottom or 18) + 4
    y = t
    for kind, a, b in (("c", "checkBadAddress", None), ("c", "checkExcludeInactive", None), ("c", "checkExcludeInsPending", None),
            ("l", "labelExcludeIfPendingDays", "textDaysInsPendingExclude"), ("c", "checkIgnoreInPerson", None),
            ("c", "checkExcludeAccountNoTil", None), ("l", "labelExcludeBalanceLessThan", "textExcludeLessThan"),
            ("l", "labelExcludeNotBilledSince", "textExcludeNotBilledSince")):
        if kind == "c":
            h.check_left(a)
            if h.cw(a) > fw - 16:
                h.set(a, 8, y, fw - 16, 34)
                y += 34 + 4
            else:
                h.set(a, 8, y, h.cw(a), CH)
                y += CH + 4
        else:
            lh = 32 if h.lw(a) > Ff - 6 - 24 else H
            h.set(a, 24, y, Ff - 6 - 24, lh)
            h.text_align(a, "MiddleLeft")
            h.set(b, Ff, y + (lh - H) // 2, h.n(b).nw, H)
            y += lh + 4
    gf = y - 4 + 8
    h.set("groupBoxFilters", fx, yA, fw, gf)
    # Assign charges
    yS = max(yA + g1 + 8 + g2, yA + gf) + 8
    t = (h.n("groupBoxAssignCharge").title_bottom or 18) + 4
    h.check_left("radioPatPriProv")
    h.set("radioPatPriProv", 8, t, h.cw("radioPatPriProv"), CH)
    h.check_left("radioSpecificProv")
    h.set("radioSpecificProv", 8, t + CH + 4 + (H - CH) // 2, h.cw("radioSpecificProv"), CH)
    h.set("comboSpecificProv", 8 + h.cw("radioSpecificProv") + 6, t + CH + 4, 174, H)
    ga = t + CH + 4 + H + 8
    aw = 8 + h.cw("radioSpecificProv") + 6 + 174 + 8
    h.set("groupBoxAssignCharge", 8, yS, aw, ga)
    for g in ("groupBox1", "groupBox2", "groupBoxFilters", "groupBoxAssignCharge"):
        h.watch(g)
    hr = max(root.h, yS + ga + 8)
    h.set("butOK", R - 75, yS + ga - 25, 75, 25)
    h.set(".", None, None, W, hr)
