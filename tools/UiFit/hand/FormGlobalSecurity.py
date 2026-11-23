"""FormGlobalSecurity on the grid (UI-12 slice 2, mode hand: the planner put the logoff minutes box over its label and left the two groups
on a narrow column). A window of mostly check boxes, laid out in the Preferences style (section 1): the check boxes one list at the content
edge on the 22 px pitch; the two field rows' labels start where the check box text starts (8 + 16), left-aligned, and their fields share one
left edge at half the content width (the logoff label wraps onto two lines there, as the 50% cap does); the Domain Login and Global Lock
groups across the content, their rows in the same style on the same field column; the last check box under them; Save at the bottom right."""


def layout(h):
    H = h.H
    CH = h.CH
    P = H + 4
    root = h.tree
    W = root.w
    R = W - 8
    F = 8 + (R - 8) // 2
    y = 8

    def check(name, x=8):
        """A check box of the list; one too wide for the content wraps onto two lines (its box at the top). Returns its height."""
        h.check_left(name)
        if h.cw(name) > R - x:
            h.set(name, x, y, R - x, 34)
            return 34
        h.set(name, x, y, h.cw(name), CH)
        return CH

    for name in ("checkTimecardSecurityEnabled", "checkCannotEditOwn", "checkCannotEditPastPayPeriods", "checkDisableBackupReminder",
            "checkLogOffWindows", "checkPasswordsMustBeStrong", "checkPasswordsStrongIncludeSpecial", "checkPasswordForceWeakToStrong",
            "checkUserNameManualEntry", "checkBadgeLogIn", "checkMaintainPatient"):
        y += check(name) + 4
    y += 4
    LX = 8 + 16
    h.set("labelPermission", LX, y, F - 6 - LX, H)
    h.text_align("labelPermission", "MiddleLeft")
    h.set("comboGroups", F, y, R - F, H)
    y += P
    h.set("label3", LX, y, F - 6 - LX, 32)
    h.text_align("label3", "MiddleLeft")
    h.set("textLogOffAfterMinutes", F, y + (32 - H) // 2, 40, H)
    y += 32 + 4
    check("checkAllowLogoffOverride")
    y += CH + 8
    gw = R - 8
    # Domain Login
    g1 = h.n("groupBox1")
    gy = (g1.title_bottom or 18) + 4
    Fg = F - 8
    h.check_left("checkDomainLoginEnabled")
    h.set("checkDomainLoginEnabled", 8, gy, h.cw("checkDomainLoginEnabled"), CH)
    gy += CH + 4
    h.set("labelDomainPath", 8 + 16, gy, Fg - 6 - 24, H)
    h.text_align("labelDomainPath", "MiddleLeft")
    h.set("textDomainLoginPath", Fg, gy, gw - 8 - Fg, H)
    gy += P
    h.check_left("checkDomainLoginForSigBoxes")
    h.set("checkDomainLoginForSigBoxes", 8, gy, h.cw("checkDomainLoginForSigBoxes"), CH)
    gy += CH + 8
    h.set("groupBox1", 8, y, gw, gy)
    h.watch("groupBox1")
    y += gy + 8
    # Global Lock
    g2 = h.n("groupBox2")
    gy = (g2.title_bottom or 18) + 4
    for lab, fld in (("label1", "textDateLock"), ("label2", "textDaysLock")):
        h.set(lab, 8 + 16, gy, Fg - 6 - 24, H)
        h.text_align(lab, "MiddleLeft")
        h.set(fld, Fg, gy, 82, H)
        if lab == "label1":
            h.set("butChange", Fg + 82 + 6, gy, 70, H)
        gy += P
    h.set("labelGlobalDateLockDisabled", 8 + 16, gy, gw - 8 - 24, 18)
    gy += 18 + 8
    h.set("groupBox2", 8, y, gw, gy)
    h.watch("groupBox2")
    y += gy + 8
    check("checkAuditPatientSelected")
    y += CH + 8
    hr = max(root.h, y + 24 + 8)
    h.set("butSave", R - 75, hr - 8 - 24, 75, 24)
    h.set(".", None, None, W, hr)
