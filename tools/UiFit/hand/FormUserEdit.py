"""FormUserEdit on the grid (UI-12 slice 3, mode hand: the planner kept the automatic logoff label over the user group caption). Main's
arrangement on the grid's numbers: the tab control at the padding down to 8 px over the bottom bar. User page: three field columns on one
pitch (UserNum, Name, Badge ID with its note; Domain User and DoseSpot User ID with their pickers; Require Password Reset and Is Hidden), the
automatic logoff label (two lines, right-aligned) before its box on the second column's field edge in a fourth row; the employee, provider
and user group lists one width and 8 px apart under their captions, the optional note centred under the first two; the permission tree in
the right column to the page's edges. Clinics and Alert Subs pages: their two lists under their captions, 8 px apart. Change Password and
Unlock Account at the bottom left, Save at the bottom right."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    HH = root.h
    H = h.H
    CH = h.CH
    by = HH - PAD - 24
    th = by - GAP - PAD
    pg = h.n("tabUser")
    ph = th - pg.ny - 2
    P = H + 4
    # Column 1.
    lw1 = max(h.lw(n) for n in ("label27", "label1", "labelBadgeId"))
    F1 = PAD + lw1 + 6
    for i, (lab, fld) in enumerate((("label27", "textUserNum"), ("label1", "textUserName"), ("labelBadgeId", "textBadgeId"))):
        y = PAD + i * P
        h.set(lab, PAD, y, lw1, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F1, y, h.n(fld).nw, H)
    bx = F1 + h.n("textBadgeId").nw + 6
    h.set("labelBadgeInstruction", bx, PAD + 2 * P, h.lw("labelBadgeInstruction"), H)
    h.text_align("labelBadgeInstruction", "MiddleLeft")
    c1 = max(F1 + h.n("textUserNum").nw, F1 + h.n("textUserName").nw, bx + h.lw("labelBadgeInstruction"))
    # Column 2.
    lw2 = max(h.lw("labelDomainUser"), h.lw("label8"))
    L2 = c1 + 16
    F2 = L2 + lw2 + 6
    fw2 = h.n("textDoseSpotUserID").nw
    for i, (lab, fld, but) in enumerate((("labelDomainUser", "textDomainUser", "butPickDomainUser"),
                                          ("label8", "textDoseSpotUserID", "butDoseSpotAdditional"))):
        y = PAD + i * P
        h.set(lab, L2, y, lw2, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F2, y, fw2, H)
        h.set(but, F2 + fw2 + 6, y, 24, H)
    c2 = F2 + fw2 + 6 + 24
    # Column 3: the two check boxes.
    C3 = c2 + 16
    for i, c in enumerate(("checkRequireReset", "checkIsHidden")):
        h.check_left(c)
        h.set(c, C3, PAD + i * P + (H - CH) // 2, h.cw(c), CH)
    # Row 4: the automatic logoff label before its box on column 2's field edge.
    lw4 = F2 - 6 - PAD
    nh = h.note_h("labelAutoLogoff", lw4)
    y4 = PAD + 3 * P
    rh = max(nh, H)
    h.set("labelAutoLogoff", PAD, y4 + (rh - nh) // 2, lw4, nh)
    h.text_align("labelAutoLogoff", "MiddleRight")
    h.set("textLogOffAfterMinutes", F2, y4 + (rh - H) // 2, h.n("textLogOffAfterMinutes").nw, H)
    # The permission tree at the right.
    tree = h.n("securityTreeUser")
    pw0 = root.w - 2 * PAD - 4
    tx = max(pw0 - PAD - tree.nw, C3 + h.cw("checkRequireReset") + GAP)
    pw = tx + tree.nw + PAD
    tw = pw + 4
    W = tw + 2 * PAD
    R = W - PAD
    h.set("tabControl1", PAD, PAD, tw, th)
    for p in ("tabUser", "tabClinics", "tabAlertSubs"):
        h.set(p, None, None, pw, ph)
    h.set("securityTreeUser", tx, PAD, tree.nw, ph - 2 * PAD)
    left_right = tx - GAP
    # The three lists.
    cy = y4 + rh + GAP
    ly = cy + 17 + 2
    note_h = 17
    lb = ph - PAD - note_h - 4
    lw = (left_right - PAD - 2 * GAP) // 3
    x = PAD
    xs = []
    for cap, lst in (("label2", "listEmployee"), ("label5", "listProv"), ("label3", "listUserGroup")):
        ww = lw if lst != "listUserGroup" else left_right - x  # the last list ends on the column's edge
        h.set(cap, x, cy, ww, 17)
        h.text_align(cap, "BottomLeft")
        h.set(lst, x, ly, ww, lb - ly)
        xs.append(x)
        x += lw + GAP
    h.set("label4", xs[0], lb + 4, xs[1] + lw - xs[0], note_h)
    h.text_align("label4", "TopCenter")
    # Clinics and Alert Subs.
    for cap1, l1, cap2, l2 in (("labelClinic", "listClinic", "label6", "listClinicMulti"),
                               ("label7", "listAlertSubMulti", "labelAlertClinic", "listAlertSubsClinicsMulti")):
        w = h.n(l1).nw
        h.set(cap1, PAD, PAD, w, 17)
        h.text_align(cap1, "BottomLeft")
        h.set(l1, PAD, PAD + 19, w, ph - PAD - (PAD + 19))
        h.set(cap2, PAD + w + GAP, PAD, w, 17)
        h.text_align(cap2, "BottomLeft")
        h.set(l2, PAD + w + GAP, PAD + 19, w, ph - PAD - (PAD + 19))
    h.set("butPassword", PAD, by, 116, 24)  # Change Password's text (101 px) filled its 103 px button
    h.set("butUnlock", PAD + 116 + 6, by, None, 24)
    h.set("butSave", R - h.n("butSave").nw, by, None, 24)
    h.set(".", None, None, W, HH)
    h.watch(".", "tabUser", "tabClinics", "tabAlertSubs")
