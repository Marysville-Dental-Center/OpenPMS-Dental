"""ControlManage (the Manage module) on the grid (UI-12 slice 5, layout script on the planner's tree). In the Time Clock group the planner
shrank Filter by Name and Reports To to 72 px (main 97) and kept the employee grid at 21 and the button column at two x (340 and 342):
the button column (buttons, server time, the status list) at one x on the content edge, the grid from the content edge to 8 px before it,
the two filter fields 120 px wide ending 8 px before it. In Messaging the Message (& Send) caption and list stood 2 and 7 px under the
other three and the History grid 7 px under the lists: the four captions on one line 2 px over the lists, the four lists from the grid's
top (under the Ack row) to one bottom, Send Message 4 px under its list."""


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
