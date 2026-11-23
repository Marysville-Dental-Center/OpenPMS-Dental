"""FormTaskEdit (Task) on the grid (UI-12 slice 2 continuation, mode hand: the reminder group's stacked panels, the repeating and object panels
and the window width the code sets from its split container). The code sets the window's width to the split container's right edge + 25
(FormTaskEdit.LoadTask: Width=splitContainer.Right+25), so everything stands within the split container's right edge, the right content
edge; the designer's client width is that edge + 8, as the code makes it. It shows the reminder group or the repeating panel (the
TasksUseRepeating preference) and the category field (the TaskCategoryShow preference), and hides the reply, the task-changed note with
Refresh and the object panel by the task's state; it sets no position.
The top band: New, Done with its note and Read Only on one row at the content edge, Date/Time Created, Date/Time Task and Date/Time Finished
under it on one field column (each label right-aligned 6 px before its box, Now 6 px after the box); 16 px after them the reminder group and
the repeating panel on one bounds with one field column (the group: Type with its box, under it the frequency panel or the date and time
pickers (on one row), the day panel under them; the panel: Date and Date Type, Is From Repeating after the type box, the date note on two lines under
them); From User (with its pick button), Task List, Task Priority (with its color button) and Category on one field column at the right
content edge. The split container 8 px under the band from the content edge to the right content edge: in its top panel the Description
label with Edit Auto Note and Auto Note under it in the date fields' label column (the hidden Red and Blue buttons on the row after them), the
description box and Short Descript's box on the date fields' column to the edge; the notes grid fills the bottom panel. Under it 8 px:
Attachments with its box (to Change's column) and button, Copy and Add at the right content edge; then Object Type with its list (both on
the date fields' column), the object panel (its label, box, Change and Go To) 16 px after the list, TaskNum at the right content edge; the
bottom bar: Delete, History (Subscribers, always hidden, after it), the task-changed note and Refresh, Send To..., Reply and its note, Save
at the bottom right."""


def lf(h, lab, fld, x, y, lw, fw, fh=None):
    H = h.H
    h.set(lab, x, y, lw, H)
    h.text_align(lab, "MiddleRight")
    h.set(fld, x + lw + 6, y, fw, H if fh is None else fh)
    return x + lw + 6 + fw


def layout(h):
    H = h.H
    CH = h.CH
    P = H + 4
    root = h.tree
    sp = h.n("splitContainer")
    W = 8 + sp.nw + 8  # the split container keeps its width; the client is its right edge + 8 (the code's Width)
    HH = root.h
    R = W - 8
    yb = HH - 8 - 24
    # Row 0: New, Done with its note, Read Only.
    x = 8
    for name in ("checkNew", "checkDone"):
        h.check_left(name)
        h.set(name, x, 8, h.cw(name), CH)
        x += h.cw(name) + 16
    x -= 16
    wa = h.lw("labelDoneAffectsAll")
    h.set("labelDoneAffectsAll", x + 2, 8, wa, CH)
    h.text_align("labelDoneAffectsAll", "MiddleLeft")
    x += 2 + wa + 16
    h.check_left("checkIsReadOnly")
    h.set("checkIsReadOnly", x, 8, h.cw("checkIsReadOnly"), CH)
    # The date rows.
    y1 = 8 + CH + 4
    lwd = max(h.lw(n) for n in ("label3", "label5", "label7"))
    lf(h, "label3", "textBoxDateTimeCreated", 8, y1, lwd, 151)
    xe = lf(h, "label5", "textDateTimeEntry", 8, y1 + P, lwd, 151)
    h.set("butNow", xe + 6, y1 + P, 62, 24)
    lf(h, "label7", "textDateTimeFinished", 8, y1 + 2 * P, lwd, 151)
    h.set("butNowFinished", xe + 6, y1 + 2 * P, 62, 24)
    yl = y1 + 2 * P + H
    # The reminder group and the repeating panel on one bounds (the code shows one or the other: TasksUseRepeating), one field column for
    # both: Type with its box; under it, inside the group's padding, the frequency panel (Every, its box and the unit) or the date and time
    # pickers, the day panel (Days, the seven boxes 14 px apart and their key) under them; in the panel Date and Date Type at the group's
    # padding, Is From Repeating after the type box, the date note on two lines under them at the column. The frequency panel and the pickers
    # are alternatives (RefreshReminderGroup: Once shows the pickers); a weekly reminder shows the day panel under the frequency panel.
    xg = xe + 6 + 62 + 16
    top = (h.n("groupReminder").title_bottom or 18) + 4
    lr = max(h.lw(n) for n in ("labelReminderRepeat", "label2", "labelReminderRepeatDays", "labelDateTask", "labelDateType"))
    Fr = 8 + lr + 6
    gw = max(8 + 159 + 6 + 96 + 8, Fr + 100 + 6 + h.cw("checkFromNum") + 8)
    lf(h, "labelReminderRepeat", "comboReminderRepeat", 8, top, lr, gw - 8 - Fr)
    y2 = top + P
    pw_ = gw - 16
    h.set("label2", 0, 0, lr, H)
    h.text_align("label2", "MiddleRight")
    h.set("textReminderRepeatFrequency", Fr - 8, 0, 50, H)
    wu = h.lw("labelRemindFrequency")
    h.set("labelRemindFrequency", Fr - 8 + 50 + 6, 0, wu, H)
    h.text_align("labelRemindFrequency", "MiddleLeft")
    h.set("panelReminderFrequency", 8, y2, pw_, H)
    h.set("datePickerReminder", 8, y2, 159, H)
    h.set("timePickerReminder", 8 + 159 + 6, y2, 96, H)
    h.set("labelReminderRepeatDays", 0, 0, lr, 15)
    h.text_align("labelReminderRepeatDays", "MiddleRight")
    for i, d in enumerate(("Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday")):
        h.set("checkReminderRepeat" + d, Fr - 8 + i * 14, 0, 13, 15)
    h.set("labelReminderRepeatDayKey", Fr - 8 - 1, 17, 109, 15)
    h.set("panelReminderDays", 8, y2 + P, pw_, 32)
    h.watch("groupReminder", "panelReminderFrequency", "panelReminderDays")
    lf(h, "labelDateTask", "textDateTask", 8, 0, lr, 100)
    lf(h, "labelDateType", "comboDateType", 8, P, lr, 100)
    h.check_left("checkFromNum")
    h.set("checkFromNum", Fr + 100 + 6, P + (H - CH) // 2, h.cw("checkFromNum"), CH)
    h.set("labelDateAdvice", Fr, 2 * P, gw - 8 - Fr, 34)
    h.text_align("labelDateAdvice", "TopLeft")
    gh2 = max(y2 + P + 32 + 8, 2 * P + 34)
    h.set("panelRepeating", xg, 8, gw, gh2)
    h.set("groupReminder", xg, 8, gw, gh2)
    h.watch("panelRepeating")
    # The right column: From User, Task List, Task Priority, Category.
    lwr = max(h.lw(n) for n in ("label16", "label10", "label8", "labelCategory"))
    fx = R - 24 - 6 - 134
    xr = fx - 6 - lwr
    lf(h, "label16", "textUser", xr, 8, lwr, 134)
    h.set("butChangeUser", R - 24, 8, 24, H)
    lf(h, "label10", "textTaskList", xr, 8 + P, lwr, 134)
    lf(h, "label8", "comboTaskPriorities", xr, 8 + 2 * P, lwr, 134)
    h.set("butColor", R - 24, 8 + 2 * P, 24, H)
    lf(h, "labelCategory", "comboCategory", xr, 8 + 3 * P, lwr, 134)
    yr = 8 + 3 * P + H
    ys = max(yl, yr, 8 + gh2) + 8
    # The bottom rows, from the bottom bar up: Attachments, then Object Type, on one field column.
    lb = max(h.lw("label6"), h.lw("labelAttachments"), lwd)
    Fb = 8 + lb + 6
    yo = yb - 8 - 43
    h.set("label6", 8, yo, lb, H)
    h.text_align("label6", "MiddleRight")
    h.set("listObjectType", Fb, yo, 120, 43)
    lod = h.lw("labelObjectDesc")
    h.set("labelObjectDesc", 0, 0, lod, H)
    h.text_align("labelObjectDesc", "MiddleRight")
    h.set("textObjectDesc", lod + 6, 0, 302, 34)
    xb = lod + 6 + 302 + 6
    h.set("butChange", xb, 5, 75, 24)
    h.set("butGoto", xb + 75 + 6, 5, 76, 24)
    pw = xb + 75 + 6 + 76
    xpo = Fb + 120 + 16
    h.set("panelObject", xpo, yo, pw, 34)
    h.watch("panelObject")
    wt = h.lw("labelTaskNum")
    h.set("textTaskNum", R - 54, yo, 54, H)
    h.set("labelTaskNum", R - 54 - 6 - wt, yo, wt, H)
    h.text_align("labelTaskNum", "MiddleRight")
    # textBox1, hidden and used by no code, stood 72 px over the window: on the TaskNum box's bounds.
    h.set("textBox1", R - 54, yo, 54, H)
    ya = yo - 4 - 24
    lf(h, "labelAttachments", "textAttachments", 8, ya, lb, xpo + xb - 6 - Fb)
    h.set("butAttachments", xpo + xb, ya, 75, 24)
    h.set("butAddNote", R - 75, ya, 75, 24)
    h.set("butCopy", R - 75 - 6 - 75, ya, 75, 24)
    yrp = ya
    # The split container between the band and the bottom rows.
    sh = yrp - 8 - ys
    h.set("splitContainer", 8, ys, sp.nw, sh)
    p1 = h.n("splitterPanel1")
    lc = lwd  # the date fields' label column: the description box on their column
    Fd = lc + 6
    h.set("label1", 0, 0, lc, H)
    h.text_align("label1", "MiddleRight")
    h.set("butEditAutoNote", 0, P, lc, 24)
    h.set("butAutoNote", 0, 2 * P, lc, 24)
    h.set("butRed", 0, 3 * P, (lc - 6) // 2, 24)
    h.set("butBlue", (lc - 6) // 2 + 6, 3 * P, (lc - 6) // 2, 24)
    ysd = p1.nh - H
    h.set("textDescript", Fd, 0, sp.nw - Fd, ysd - 4)
    h.set("label4", 0, ysd, lc, H)
    h.text_align("label4", "MiddleRight")
    h.set("textDescriptOverride", Fd, ysd, sp.nw - Fd, H)
    p2 = h.n("splitterPanel2")
    h.set("splitterPanel2", None, None, sp.nw, sh - p2.ny)
    h.set("gridMain", 0, 0, sp.nw, sh - p2.ny)
    # The bottom bar.
    h.set("butDelete", 8, yb, 80, 24)
    h.set("butAudit", 8 + 80 + 6, yb, 75, 24)
    xs = 8 + 80 + 6 + 75 + 6
    h.set("butSubscribers", xs, yb, 75, 24)
    xs += 75 + 6
    wc = h.lw("labelTaskChanged")
    h.set("labelTaskChanged", xs, yb, wc, 24)
    h.text_align("labelTaskChanged", "MiddleRight")
    xs += wc + 6
    h.set("butRefresh", xs, yb, 75, 24)
    xs += 75 + 16
    h.set("butSend", xs, yb, 75, 24)
    xs += 75 + 6
    h.set("butReply", xs, yb, 75, 24)
    xs += 75 + 6
    h.set("labelReply", xs, yb, h.lw("labelReply"), 24)
    h.text_align("labelReply", "MiddleLeft")
    h.set("butSave", R - 75, yb, 75, 24)
    h.set(".", None, None, W, HH)
