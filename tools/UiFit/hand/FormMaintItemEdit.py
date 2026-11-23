"""FormMaintItemEdit on the grid (UI-12 slice 3, mode hand: the planner left the weekday boxes off the field column and the two grids'
button rows at two x). Main's arrangement on the grid's numbers, one field column for the window and the Schedule group: Name,
Description, Equipment; the Schedule group (Repeats; Every with its unit and the after note; On with the seven day boxes; First due date;
Remind with its note; the next due line, set by the code, under them); Assigned to, Hidden and the unassigned note (shown by the code)
under it. The right column 16 px after the group to the right edge: the checklist grid and the result field grid one height, each with Add,
Edit and Delete under it at its left and Up and Down at its right edge. Delete and Save on the bottom bar."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    CH = h.CH
    P = H + 4
    inner = ["labelScheduleType", "labelEvery", "labelWeekdays", "labelAnchor", "labelLead"]
    ilw = max(h.lw(n) for n in inner + ["labelName", "labelDescription", "labelEquipment", "labelAssigned"])
    Fi = PAD + ilw + 6
    F = PAD + Fi
    olw = F - 6 - PAD
    days = ("checkSun", "checkMon", "checkTue", "checkWed", "checkThu", "checkFri", "checkSat")
    x = Fi
    for d in days:
        x += h.cw(d) + GAP
    gw = x - GAP + PAD
    LR = PAD + gw
    by = HH - PAD - 24
    # The left column.
    y = PAD
    for lab, fld in (("labelName", "textName"), ("labelDescription", "textDescription"), ("labelEquipment", "comboEquipment")):
        fh = h.n(fld).nh if fld == "textDescription" else H
        h.set(lab, PAD, y, olw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F, y, LR - F, fh)
        y += fh + 4
    y += GAP - 4
    g = h.n("groupSchedule")
    top = (g.title_bottom or 18) + 4
    cr = gw - PAD
    yy = top

    def lab(name):
        h.set(name, PAD, yy, ilw, H)
        h.text_align(name, "MiddleRight")

    lab("labelScheduleType")
    h.set("comboScheduleType", Fi, yy, cr - Fi, H)
    yy += P
    lab("labelEvery")
    h.set("textCount", Fi, yy, 50, H)
    h.set("comboUnit", Fi + 56, yy, 100, H)
    h.set("labelAfter", Fi + 162, yy, cr - (Fi + 162), H)
    h.text_align("labelAfter", "MiddleLeft")
    yy += P
    lab("labelWeekdays")
    x = Fi
    for d in days:
        h.check_left(d)
        h.set(d, x, yy + (H - CH) // 2, h.cw(d), CH)
        x += h.cw(d) + GAP
    yy += P
    lab("labelAnchor")
    h.set("textAnchor", Fi, yy, 100, H)
    yy += P
    lab("labelLead")
    h.set("textLead", Fi, yy, 50, H)
    h.set("labelLeadDays", Fi + 56, yy, cr - (Fi + 56), H)
    h.text_align("labelLeadDays", "MiddleLeft")
    yy += P
    h.set("labelNextDue", PAD, yy, cr - PAD, 30)
    gh = yy + 30 + PAD
    h.set("groupSchedule", PAD, y, gw, gh)
    y += gh + GAP
    h.set("labelAssigned", PAD, y, olw, H)
    h.text_align("labelAssigned", "MiddleRight")
    h.set("comboAssigned", F, y, LR - F, H)
    y += P
    h.check_left("checkHidden")
    h.set("checkHidden", F, y, h.cw("checkHidden"), CH)
    y += CH + 2
    nh = h.note_h("labelUnassigned", LR - F)
    h.set("labelUnassigned", F, y, LR - F, nh)
    h.text_align("labelUnassigned", "TopLeft")
    # The right column.
    rx = LR + 16
    rw = R - rx
    gh2 = (by - GAP - PAD - GAP - 2 * (4 + 24)) // 2
    yg = PAD
    for grid, left, right in (("gridSteps", ("butStepAdd", "butStepEdit", "butStepDelete"), ("butStepUp", "butStepDown")),
                              ("gridFields", ("butFieldAdd", "butFieldEdit", "butFieldDelete"), ("butFieldUp", "butFieldDown"))):
        h.set(grid, rx, yg, rw, gh2)
        yb = yg + gh2 + 4
        x = rx
        for b in left:
            h.set(b, x, yb, 75, 24)
            x += 81
        x = R
        for b in reversed(right):
            x -= 75
            h.set(b, x, yb, 75, 24)
            x -= 6
        yg = yb + 24 + GAP
    h.set("butDelete", PAD, by, 80, 24)
    h.set("butSave", R - 75, by, 75, 24)
    h.watch(".", "groupSchedule")
