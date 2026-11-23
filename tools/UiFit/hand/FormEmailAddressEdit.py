"""FormEmailAddressEdit on the grid (UI-12 slice 3, mode hand). FormEmailAddressEdit.cs lays the Email Authentication box out at run time
(LayoutAuthentication, S0-F) from EmailAuthenticationLayout's numbers, which now come from the grid (TopRadios, StepRadios, LeftDetails,
TopPassword, TopSignInBox, MarginBottom, GapBelow; S0FTests' EmailAddressLayoutTests check the designer against them). The designer: Username
with its box, Use SSL under the box; the Email Authentication group at the padding: the three choices a list at the content edge, the
password panel at (LeftDetails, TopPassword) (its label and box on the Password choice's row), the Microsoft 365 box under it and the Gmail
box at TopSignInBox right of them (the code puts the chosen one at LeftDetails), the group as tall as with Gmail chosen; the incoming and
outgoing settings side by side GapBelow under it, one height, one field column, the example notes under the server boxes, the port and
sender notes after their boxes; the user panel under the incoming settings with its box on their column; Delete and Certificate at the
bottom left, Save at the bottom right. Inside the sign-in boxes: the buttons, the status and the inbox search box on one column, the status
caption and the search caption before it, the download check box on the column with its note after it."""

PAD = 8
GAP = 8


def signin_box(h, grp, lab_status, status, buttons_before, but_after, check, note, extra=(), cap_w=None):
    """Lays a sign-in group out from its title band; returns (width, height)."""
    H = h.H
    CH = h.CH
    g = h.n(grp)
    top = (g.title_bottom or 18) + 4
    lw = cap_w or h.lw(lab_status)
    F = PAD + lw + 6
    y = top
    for b in buttons_before:
        h.set(b, F, y, 115, 24)
        y += 24 + 4
    sh = h.n(status).nh
    h.set(lab_status, PAD, y, lw, 20)
    h.text_align(lab_status, "MiddleRight")
    h.set(status, F, y + 2, h.n(status).nw, sh)
    y += sh + 4
    h.set(but_after, F, y, 115, 24)
    y += 24 + 4
    h.check_left(check)
    h.set(check, F, y, h.cw(check), CH)
    nx = F + h.cw(check) + 6
    h.set(note, nx, y, h.lw(note), CH)
    h.text_align(note, "MiddleLeft")
    right = nx + h.lw(note)
    y += CH
    for kind, a, b in extra:
        y += 4
        if kind == "captioned":
            fh = h.n(b).nh
            h.set(a, PAD, y, lw, h.n(a).nh)
            h.text_align(a, "TopRight")
            h.set(b, F, y, h.n(b).nw, fh)
            right = max(right, F + h.n(b).nw)
            y += max(fh, h.n(a).nh)
        else:
            h.check_left(a)
            h.set(a, F, y, h.cw(a), CH)
            y += CH
    return right + PAD, y + PAD


def layout(h):
    root = h.tree
    H = h.H
    CH = h.CH
    # Username and Use SSL.
    lw = h.lw("label3")
    F0 = PAD + lw + 6
    h.set("label3", PAD, PAD, lw, H)
    h.text_align("label3", "MiddleRight")
    h.set("textUsername", F0, PAD, h.n("textUsername").nw, H)
    h.check_left("checkSSL")
    h.set("checkSSL", F0, PAD + H + 4, h.cw("checkSSL"), CH)
    # The authentication group.
    ga = h.n("groupAuthentication")
    top = (ga.title_bottom or 18) + 4  # TopRadios
    step = CH + 4  # StepRadios
    for i, r in enumerate(("radioPassword", "radioGmail", "radioMicrosoft")):
        h.check_left(r)
        h.set(r, PAD, top + i * step, h.cw(r), CH)
    left = PAD + max(h.cw(r) for r in ("radioPassword", "radioGmail", "radioMicrosoft")) + 16  # LeftDetails
    toppw = top - (H - CH) // 2  # TopPassword: the password row on the Password choice's row
    plw = h.lw("labelPassword")
    h.set("labelPassword", 0, 0, plw, H)
    h.text_align("labelPassword", "MiddleRight")
    h.set("textPassword", plw + 6, 0, h.n("textPassword").nw, H)
    pw = plw + 6 + h.n("textPassword").nw
    h.set("panelPassword", left, toppw, pw, H)
    cap_ms = h.lw("labelMicrosoftStatusCaption")
    mw, mh = signin_box(h, "groupMicrosoft", "labelMicrosoftStatusCaption", "labelMicrosoftStatus", ["butAuthMicrosoft"],
                        "butClearTokensMicrosoft", "checkDownloadMicrosoft", "labelDownloadMicrosoft", cap_w=cap_ms)
    my = toppw + H + GAP
    h.set("groupMicrosoft", left, my, mw, mh)
    gx = left + max(pw, mw) + 16
    capg = h.n("labelParams").nw
    gw, gh = signin_box(h, "groupGmail", "labelGmailStatusCaption", "labelGmailStatus", ["butAuthGoogle"], "butClearTokensGmail",
                        "checkDownloadGmail", "labelDownloadGmail", extra=[("captioned", "labelParams", "textParams"),
                                                                           ("check", "checkUnreadGmail", None)], cap_w=capg)
    h.set("groupGmail", gx, top, gw, gh)  # TopSignInBox: the content's top
    aw = gx + gw + PAD
    ah = max(top + 2 * step + CH, top + gh, my + mh) + PAD  # MarginBottom
    ay = PAD + H + 4 + CH + GAP
    W = max(root.w, PAD + aw + PAD)
    R = W - PAD
    h.set("groupAuthentication", PAD, ay, R - PAD, ah)
    # Incoming and outgoing settings.
    sy = ay + ah + GAP  # GapBelow
    labels = ["label8", "label11", "label1", "label5", "label2"]
    slw = max(h.lw(n) for n in labels)
    SF = PAD + slw + 6
    gin = h.n("groupIncoming")
    stop = (gin.title_bottom or 18) + 4

    def settings(rows):
        y = stop
        right = 0
        prev_w = 0
        for kind, lab, fld, note in rows:
            if kind == "field":
                h.set(lab, PAD, y, slw, H)
                h.text_align(lab, "MiddleRight")
                f = h.n(fld)
                h.set(fld, SF, y, f.nw, H)
                prev_w = f.nw
                right = max(right, SF + f.nw)
                if note:
                    h.set(note, SF + f.nw + 6, y, h.lw(note), H)
                    h.text_align(note, "MiddleLeft")
                    right = max(right, SF + f.nw + 6 + h.lw(note))
                y += H + 4
            else:  # an example note under the box before it
                n = h.n(lab)
                lines = n.text.replace("\\n", "\n").count("\n") + 1
                nh = lines * 16 + 2
                h.set(lab, SF, y - 2, prev_w, nh)
                h.text_align(lab, "TopLeft")
                y += nh + 2
        return right + PAD, y - 4 + PAD

    iw, ih = settings([("field", "label8", "textSMTPserverIncoming", None), ("note", "label14", None, None),
                       ("field", "label11", "textPortIncoming", "label10")])
    ow, oh = settings([("field", "label1", "textSMTPserver", None), ("note", "label13", None, None),
                       ("field", "label5", "textPort", "label6"), ("field", "label2", "textSender", "label9")])
    gh2 = max(ih, oh)
    h.set("groupIncoming", PAD, sy, iw, gh2)
    h.set("groupOutgoing", PAD + iw + GAP, sy, ow, gh2)
    # The user panel under the incoming settings, its box on their column.
    uy = sy + gh2 + GAP
    ulw = h.lw("labelUserod")
    h.set("labelUserod", SF - 6 - ulw, 0, ulw, H)
    h.text_align("labelUserod", "MiddleRight")
    h.set("textUserod", SF, 0, h.n("textUserod").nw, H)
    h.set("butPickUserod", SF + h.n("textUserod").nw + 6, 0, 24, H)
    h.set("panelUserod", PAD, uy, SF + h.n("textUserod").nw + 6 + 24, H)
    by = uy + H + GAP
    HH = max(root.h, by + 24 + PAD)
    by = HH - PAD - 24
    h.set("butDelete", PAD, by, None, 24)
    h.set("butRegisterCertificate", PAD + h.n("butDelete").nw + 6, by, None, 24)
    h.set("butSave", R - h.n("butSave").nw, by, None, 24)
    h.set(".", None, None, W, HH)
    h.watch(".", "groupAuthentication", "groupMicrosoft", "groupGmail", "groupIncoming", "groupOutgoing", "panelUserod", "panelPassword")
    print("EmailAuthenticationLayout: TopRadios=%d StepRadios=%d LeftDetails=%d TopPassword=%d TopSignInBox=%d MarginBottom=%d GapBelow=%d"
          % (top, step, left, toppw, top, PAD, GAP))
