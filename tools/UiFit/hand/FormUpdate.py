"""FormUpdate on the grid (UI-12 slice 14, layout script). Its code shows one state of its panels: the fork's Load hides the classic
check (panelClassic) and the second Check button and shows the version line and the message that updates come from the database; the
version groups (Build, Stable, Beta, Alpha) and the classic panel are Open Dental's states, which no fork code shows (kept, not cut). Main:
one 480 px column in the middle of the 647 px window, the classic panel outside the window (605, 35), the groups' rows 2 to 6 px from
their edges. Now, on the padding over the window's width: the version line under the menu, Check for Updates (hidden) and Show Update
History on the next row (left and right), the message under them; under the message one area that each state fills: the four version
groups stacked 8 px apart (each a caption, its version box and its Download msi and Install buttons on the right; Beta's acknowledge
check box beside its buttons) or the classic panel (its note, the Website Path and Update Code rows, Check for Updates, the result boxes,
Download). The window grows 23 px so the four groups fit under the message (their place is in the window: GridRulesTests' hidden-frame)."""

PAD = 8
GAP = 4
MENU = 24


def version_group(h, g, cap, text, dl, inst, w, check=None):
    cw = w - 2 * PAD
    ch = h.note_h(cap, cw)
    h.text_align(cap, "BottomLeft")
    h.set(cap, PAD, 22, cw, ch)
    y = 22 + ch + 2
    h.set(text, PAD, y, cw, h.H)
    y += h.H + GAP
    h.set(inst, w - PAD - 74, y, 74, h.H)
    h.set(dl, w - PAD - 74 - 8 - 84, y, 84, h.H)
    if check:
        h.check_left(check)
        h.set(check, PAD, y + 2, h.cw(check), 18)
    return y + h.H + PAD


def layout(h):
    H = h.H
    W = h.tree.nw
    R = W - PAD
    cw = R - PAD
    y = MENU + PAD
    h.text_align("labelVersion", "MiddleLeft")
    h.set("labelVersion", PAD, y, cw, H)
    y += H + GAP
    h.set("butCheck2", PAD, y, 120, H)
    h.set("butShowUpdateHistory", R - 132, y, 132, H)
    y += H + GAP
    h.set("textConnectionMessage", PAD, y, cw, 66)
    y += 66 + 8
    top = y
    for g, args in (("groupBuild", ("label2", "textBuild", "butDownloadMsiBuild", "butInstallBuild")),
                    ("groupStable", ("label11", "textStable", "butDownloadMsiStable", "butInstallStable")),
                    ("groupBeta", ("label5", "textBeta", "butDownloadMsiBeta", "butInstallBeta")),
                    ("groupAlpha", ("label7", "textAlpha", "butDownloadMsiAlpha", "butInstallAlpha"))):
        gh = version_group(h, g, *args, cw, check="checkAcknowledgeBeta" if g == "groupBeta" else None)
        h.set(g, PAD, y, cw, gh)
        y += gh + 8
    bottom = y - 8 + PAD
    # the classic panel, in the same place as the groups (shown instead of them)
    nh = h.note_h("label6", cw - 2 * PAD)
    h.set("label6", PAD, PAD, cw - 2 * PAD, nh)
    yc = PAD + nh + 8
    lw = max(h.lw("label3"), h.lw("label4"))
    fx = PAD + lw + 6
    fw = cw - PAD - fx
    for lb, f, w in (("label3", "textWebsitePath", fw), ("label4", "textUpdateCode", 112)):
        h.text_align(lb, "MiddleRight")
        h.set(lb, PAD, yc, lw, H)
        h.set(f, fx, yc, w, H)
        yc += 27
    h.set("butCheck", fx, yc, 120, H)
    yc += 27
    h.set("textResult", fx, yc, fw, H)
    yc += 27
    h.set("textResult2", fx, yc, fw, 66)
    yc += 66 + GAP
    h.set("butDownload", fx, yc, 84, H)
    h.set("panelClassic", PAD, top, cw, yc + H + PAD)
    h.set(".", None, None, W, max(h.tree.nh, bottom))
