"""FormClockEventEdit on the grid (UI-12 slice 3, mode hand: the planner kept the groups at 80 and Delete at 31). Main's arrangement on the
grid's numbers, everything from the padding: the clinic picker, Out Status and Note on one field column whose labels end where the groups'
labels end; the Clock In and Clock Out groups side by side, one width and height (Entered, Displayed, Now and Clear on the groups' field
column); the Time Spans and Rate 2 or Rate 3 groups side by side, one width and height, the Calculated and Override captions over their
columns and the rows on one pitch in both (the first row as tall as Total Time's two lines); Working From Home (shown by the code) 16 px
after the status list; Delete and Save on the bottom bar."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    H = h.H
    CH = h.CH
    inner = ["label1", "label2", "label5", "label6", "label8", "label10", "label7", "label9", "labelRate2", "labelRate3", "label15"]
    ilw = max(max(h.lw(n) for n in inner), 72)
    Fi = PAD + ilw + 6  # the groups' field column (group coordinates)
    F = PAD + Fi  # the window's field column
    gw = 323
    W = PAD + gw + GAP + gw + PAD
    R = W - PAD
    x2 = PAD + gw + GAP
    olw = F - 6 - PAD
    # The clinic picker.
    cl = h.clinic_w("comboClinic")
    h.set("comboClinic", F - cl, PAD, 168 + cl, H)  # main's 168 px box on the field column
    y = PAD + H + GAP
    # Clock In / Clock Out.
    fw = 156
    for grp, rows, buts in (("groupBox1", (("label1", "textTimeEntered1"), ("label2", "textTimeDisplayed1")), ("butNow1",)),
                            ("groupBox2", (("label5", "textTimeEntered2"), ("label6", "textTimeDisplayed2")), ("butNow2", "butClear"))):
        g = h.n(grp)
        top = (g.title_bottom or 18) + 4
        yy = top
        for lab, fld in rows:
            h.set(lab, PAD, yy, ilw, H)
            h.text_align(lab, "MiddleRight")
            h.set(fld, Fi, yy, fw, H)
            yy += H + 4
        x = Fi
        for b in buts:
            bw = 70 if b != "butClear" else 80
            h.set(b, x, yy, bw, 24)
            x += bw + 6
        gh = yy + 24 + PAD
    h.set("groupBox1", PAD, y, gw, gh)
    h.set("groupBox2", x2, y, gw, gh)
    y += gh + GAP
    # Out Status.
    h.set("label3", PAD, y, olw, H)
    h.text_align("label3", "MiddleRight")
    ls = h.n("listStatus")
    h.set("listStatus", F, y, ls.nw, ls.nh)
    h.check_left("checkIsWorkingHome")
    h.set("checkIsWorkingHome", F + ls.nw + 16, y + (H - CH) // 2, h.cw("checkIsWorkingHome"), CH)
    y += ls.nh + GAP
    # Time Spans / Rate 2 or Rate 3.
    cw = 68
    c2 = Fi + cw + 6
    r1h = h.note_h("label14", ilw)
    r1h = max(r1h, H)
    for grp, caps, rows in (("groupTimeSpans", ("label11", "label12"),
                             (("label8", "textClockedTime", None), ("label10", "textAdjustAuto", "textAdjust"),
                              ("label7", "textOTimeAuto", "textOTimeHours"), ("label9", "textRegTime", None))),
                            ("groupRate2orRate3", ("label18", "label17"),
                             (("label14", "textTotalHours", None), ("labelRate2", "textRate2Auto", "textRate2Hours"),
                              ("labelRate3", "textRate3Auto", "textRate3Hours"), ("label15", "textRate1Auto", None)))):
        g = h.n(grp)
        top = (g.title_bottom or 18) + 4
        h.set(caps[0], Fi, top, cw, 17)
        h.text_align(caps[0], "BottomCenter")
        h.set(caps[1], c2, top, cw, 17)
        h.text_align(caps[1], "BottomCenter")
        yy = top + 17 + 2
        for i, (lab, a, b) in enumerate(rows):
            rh = r1h if i == 0 else H
            lh = h.note_h(lab, ilw) if lab == "label14" else H
            h.set(lab, PAD, yy + (rh - lh) // 2, ilw, lh)
            h.text_align(lab, "MiddleRight")
            h.set(a, Fi, yy + (rh - H) // 2, cw, H)
            if b:
                h.set(b, c2, yy + (rh - H) // 2, cw, H)
            yy += rh + 4
        th = yy - 4 + PAD
    h.set("groupTimeSpans", PAD, y, gw, th)
    h.set("groupRate2orRate3", x2, y, gw, th)
    y += th + GAP
    # Note.
    h.set("label4", PAD, y, olw, H)
    h.text_align("label4", "MiddleRight")
    tn = h.n("textNote")
    h.set("textNote", F, y, R - F, tn.nh)
    y += tn.nh + GAP
    HH = max(root.h, y + 24 + PAD)
    h.set("butDelete", PAD, HH - PAD - 24, None, 24)
    h.set("butSave", R - h.n("butSave").nw, HH - PAD - 24, None, 24)
    h.set(".", None, None, W, HH)
    h.watch(".", "groupBox1", "groupBox2", "groupTimeSpans", "groupRate2orRate3")
