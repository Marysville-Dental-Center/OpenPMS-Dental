"""ControlManage (the Manage module) on the grid (UI-12 slice 5, layout script on the planner's tree). In the Time Clock group the planner
shrank Filter by Name and Reports To to 72 px (main 97) and kept the employee grid at 21 and the button column at two x (340 and 342):
the button column (buttons, server time, the status list) at one x on the content edge, the grid from the content edge to 8 px before it,
the two filter fields 120 px wide ending 8 px before it. In Messaging the Message (& Send) caption and list stood 2 and 7 px under the
other three and the History grid 7 px under the lists: the four captions on one line 2 px over the lists, the four lists from the grid's
top (under the Ack row) to one bottom, Send Message 4 px under its list.

Fix round 2: in the Daily group the two button columns (104 px at 16 and 126) left 42 px on the right against 16 on the left: they fill
the group, 117 px each at 16 and 139 with the 6 px gap, 16 px on both sides. In Messaging the Message (& Send) caption, its list, Send
Message and the Sending note under it stood 13 px after Extras (the other lists 6 px apart): 6 px after it, the list (and Send) widened
to end 6 px before the History grid."""


def layout(h):
    g = h.n("groupBox1")
    bx = g.nw - 8 - 108
    for name in ("butManage", "butTimeCard", "butBreaks", "butViewSched", "butClockIn", "butClockOut", "listBoxStatus", "labelCurrentTime",
                 "labelTime"):
        h.set(name, bx, None, 108, None)
    h.set("gridEmp", 8, None, bx - 8 - 8, None)
    # The server time (a bold 12:00:00 PM, 111 x 23 measured) centred on the column, 2 px past it on each side.
    h.set("labelTime", bx - 2, None, 112, 23)
    fw = 120
    for lab, fld in (("labelFilterName", "textFilterName"), ("labelReportsTo", "comboReportsTo")):
        h.set(fld, bx - 8 - fw, None, fw, None)
        lw = h.lw(lab)
        h.set(lab, bx - 8 - fw - 6 - lw, None, lw, None)
    top = h.n("gridMessages").ny
    lst = h.n("listBoxTo")
    bottom = lst.ny + lst.nh
    for cap, name in (("label1", "listBoxTo"), ("label3", "listBoxFrom"), ("label4", "listBoxExtras"), ("label5", "listBoxMessages")):
        c = h.n(cap)
        h.set(cap, None, top - 2 - 18, None, 18)
        h.set(name, None, top, None, bottom - top)
    h.set("butSend", None, bottom + 4)
    # The Daily group's two button columns fill it: 16 px margins, a 6 px gap.
    d = h.n("groupBox3")
    cw = (d.nw - 16 - 16 - 6) // 2
    for name in ("butSendClaims", "butClaimPay", "butBilling", "butDeposit", "butSupply", "butManageAR", "butMaintenance"):
        h.set(name, 16, None, cw, None)
    for name in ("butTasks", "butBackup", "butAccounting", "butEmailInbox", "butEras", "butImportInsPlans"):
        h.set(name, 16 + cw + 6, None, cw, None)
    # The message list 6 px after Extras, to 6 px before the History grid.
    ex = h.n("listBoxExtras")
    mx = ex.nx + ex.nw + 6
    mw = h.n("gridMessages").nx - 6 - mx
    h.set("label5", mx)
    h.set("listBoxMessages", mx, None, mw, None)
    h.set("butSend", mx, None, mw, None)
    h.set("labelSending", mx, None, mw, None)
