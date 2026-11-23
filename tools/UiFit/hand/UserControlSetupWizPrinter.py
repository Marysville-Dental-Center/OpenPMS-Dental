"""UserControlSetupWizPrinter (the setup wizard's printer page) on the grid (UI-12 slice 5, layout script on the planner's tree). The page
keeps its 930 px. Its printer rows live in panelSimple, which "Show me the simple interface" hides (UserControlSetupWizPrinter.cs), while
the Default row's combo box (comboDefault) and the third note line (label1) are the group's own children standing on the panel's rows; the
planner took the panel and the combo box apart and pushed the scanner group past the page, cutting its texts to three letters. Main's
arrangement on the grid: the page note; the Printer Setup group (its two-line note, the third note line with the Prompt caption over the
check boxes' column on its baseline, the twelve rows on the 27 px pitch with the labels right-aligned 6 px before the combo boxes and the
prompt check boxes 8 px after them, Default's combo box on the first row, Show me the simple interface under the panel) and the Scanner
Setup group 8 px beside it, one top and one height (Set Default Scanner; Show Select Scanner Window with its box on the left; the JPEG
quality row, its label where a check box's text starts, the box and its info icon after it; the two radio buttons as a list; the Scanning
Options group with Duplex (and its info icon), Grayscale and the Resolution row on the JPEG row's field column; the closing note and
Advanced at the group's bottom)."""


ROWS = (("labelDefault", None, "checkDefault"), ("label12", "comboAppointments", "checkAppointments"), ("label4", "comboClaim", "checkClaim"),
        ("label5", "comboLabelSheet", "checkLabelSheet"), ("label8", "comboLabelSingle", "checkLabelSingle"),
        ("label9", "comboPostcard", "checkPostcard"), ("label6", "comboRx", "checkRx"), ("label13", "comboRxControlled", "checkRxControlled"),
        ("labelRxMulti", "comboRxMulti", "checkRxMulti"), ("label3", "comboStatement", "checkStatement"),
        ("labelTPandPerio", "comboTPPerio", "checkTPPerio"), ("label14", "comboReceipt", "checkReceipt"))


def layout(h):
    H = h.H
    P = H + 4
    CH = h.CH
    W = h.tree.nw
    h.set("label2", 8, 8, None, 31)
    gy = 8 + 31 + 8
    # Printer Setup
    CX, CW = 206, 252
    KX = CX + CW + 8
    PW = KX + 24 + 8
    GW1 = 8 + PW + 8
    h.set("label11", 8, 22, GW1 - 16, 32)
    h.text_align("label11", "TopLeft")
    h.set("label1", 8, 58, 382, 18)
    h.text_align("label1", "MiddleLeft")
    PY = 58 + 18 - 23
    h.set("labelPrompt", KX + 12 - 26, 0, 53, 23)
    y = 23 + 4
    for lab, combo, chk in ROWS:
        h.set(lab, 0, y, CX - 6, H)
        h.text_align(lab, "MiddleRight")
        if combo:
            h.set(combo, CX, y, CW, H)
        else:
            h.set("comboDefault", 8 + CX, PY + y, CW, H)
        k = h.n(chk)
        h.set(chk, KX, y + (H - k.nh) // 2, None, None)
        y += P
    ph = y - 4
    h.set("panelSimple", 8, PY, PW, ph)
    ys = PY + ph + 4
    h.check_left("checkSimple")
    h.set("checkSimple", 8, ys, h.cw("checkSimple"), CH)
    GH = ys + CH + 8
    h.set("groupPrinter", 8, gy, GW1, GH)
    # Scanner Setup
    x2 = 8 + GW1 + 8
    GW2 = W - 8 - x2
    h.set("groupBox5", x2, gy, GW2, GH)
    h.set("butSetScanner", 8, 22, None, 24)
    y = 22 + 24 + 8
    h.check_left("checkScanDocSelectSource")
    h.set("checkScanDocSelectSource", 8, y, h.cw("checkScanDocSelectSource"), CH)
    y += CH + 4
    lw = h.lw("label21")
    fx = 24 + lw + 6
    h.set("label21", 24, y, lw, H)
    h.text_align("label21", "MiddleLeft")
    h.set("textScanDocQuality", fx, y, None, H)
    q = h.n("textScanDocQuality")
    h.set("labelInfoCompression", fx + q.nw + 6, y + 1, 20, 20)
    y += P
    for name in ("radioScanDocShowOptions", "radioScanDocUseOptionsBelow"):
        h.check_left(name)
        h.set(name, 8, y, h.cw(name), CH)
        y += CH + 4
    so = y
    h.set("checkScanDocDuplex", 8, 22, h.cw("checkScanDocDuplex"), CH)
    h.check_left("checkScanDocDuplex")
    h.set("labelInfoDuplex", 8 + h.cw("checkScanDocDuplex") + 6, 21, 20, 20)
    h.check_left("checkScanDocGrayscale")
    h.set("checkScanDocGrayscale", 8, 44, h.cw("checkScanDocGrayscale"), CH)
    lr = h.lw("label18")
    h.set("label18", 24, 66, lr, H)
    h.text_align("label18", "MiddleLeft")
    r = h.n("textScanDocResolution")
    h.set("textScanDocResolution", fx - 8, 66, None, H)
    h.set("labelInfoResolution", fx - 8 + r.nw + 6, 67, 20, 20)
    h.set("groupScanningOptions", 8, so, GW2 - 16, 66 + H + 8)
    a = h.n("butAdvanced")
    ya = GH - 8 - a.nh
    h.set("butAdvanced", GW2 - 8 - a.nw, ya)
    h.set("label7", 8, ya - 4 - 34, GW2 - 16, 34)
