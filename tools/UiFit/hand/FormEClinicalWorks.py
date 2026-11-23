"""FormEClinicalWorks (the eClinicalWorks program link) on the grid (UI-12 slice 3, mode hand: the planner left the HL7 server row taller
than its pitch and the HL7 Sync Folders group 34 px in). Main's arrangement on the grid's numbers, one field column for the window and its
group: Internal Name and Description; Enabled and the three modes a list on the column; the four server rows and Medical Panel URL with their
notes 6 px after the boxes on one line (the window 742 px wide for the HL7 server's); Exclude LBSESSIONID with its note; the HL7 definition
warning (shown by the code) in its own row; the HL7 Sync Folders group at the padding to the right edge (its note, In and Out on the
column); Default User Group; the six check boxes a list on the column (the HL7 note after Patient Fee Schedules); Diagnostic Tool at the
bottom left, Save at the bottom right."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    H = h.H
    CH = h.CH
    labels = ["label1", "label2", "label3", "label6", "label7", "label4", "label11", "labelDefaultUserGroup", "labelHL7FolderIn",
              "labelHL7FolderOut"]
    lw = max(h.lw(n) for n in labels)
    F = PAD + lw + 6
    # The window as wide as the notes after the boxes on one line (the HL7 server's wrapped to two, its row off the pitch).
    W = max(root.w, F + 181 + 6 + h.lw("label8") + PAD, F + 287 + 6 + h.lw("label10") + PAD)
    R = W - PAD
    y = PAD

    def field(lab, fld, w, note=None):
        nonlocal y
        nh = 0
        nx = F + w + 6
        if note:
            nh = h.note_h(note, R - nx) if h.n(note).text_w + 4 > R - nx else H
        rh = max(H, nh)
        h.set(lab, PAD, y + (rh - H) // 2, lw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F, y + (rh - H) // 2, w, H)
        if note:
            h.set(note, nx, y + (rh - nh) // 2, R - nx if nh > H else h.lw(note), nh)
            h.text_align(note, "MiddleLeft")
        y += rh + 4

    def check(name, note=None):
        nonlocal y
        h.check_left(name)
        h.set(name, F, y, h.cw(name), CH)
        if note:
            h.set(note, F + h.cw(name) + 6, y, h.lw(note), CH)
            h.text_align(note, "MiddleLeft")
        y += CH + 4

    field("label1", "textProgName", 275)
    field("label2", "textProgDesc", 275)
    for c in ("checkEnabled", "radioModeTight", "radioModeStandalone", "radioModeFull"):
        check(c)
    field("label3", "textECWServer", 181)
    field("label6", "textODServer", 181)
    field("label7", "textHL7Server", 181, "label8")
    field("label4", "textHL7ServiceName", 181, "label9")
    field("label11", "textMedPanelURL", 287, "label10")
    check("checkLBSessionId", "label12")
    wh = h.note_h("labelDefEnabledWarning", R - PAD)
    h.set("labelDefEnabledWarning", PAD, y, R - PAD, wh)
    h.text_align("labelDefEnabledWarning", "MiddleLeft")
    y += wh + GAP - 4
    g = h.n("groupBox1")
    top = (g.title_bottom or 18) + 4
    gw = R - PAD
    nh = h.note_h("label5", gw - 2 * PAD)
    h.set("label5", PAD, top, gw - 2 * PAD, nh)
    h.text_align("label5", "TopLeft")
    yy = top + nh + 4
    for lab, fld in (("labelHL7FolderIn", "textHL7FolderIn"), ("labelHL7FolderOut", "textHL7FolderOut")):
        h.set(lab, PAD, yy, lw - PAD, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F - PAD, yy, 275, H)
        yy += H + 4
    gh = yy - 4 + PAD
    h.set("groupBox1", PAD, y, gw, gh)
    y += gh + GAP
    field("labelDefaultUserGroup", "comboDefaultUserGroup", 215)
    for c, n in (("checkShowImages", None), ("checkFeeSchedules", "labelHL7Warning"), ("checkQuadAsToothNum", None),
                 ("checkHideButChartRx", None), ("checkProcRequireSignature", None), ("checkProcNotesNoIncomplete", None)):
        check(c, n)
    y += GAP - 4
    HH = max(root.h, y + 24 + PAD)
    h.set("butDiagnostic", PAD, HH - PAD - 24, 100, 24)
    h.set("butSave", R - h.n("butSave").nw, HH - PAD - 24, None, 24)
    h.set(".", None, None, W, HH)
    h.watch(".", "groupBox1")
