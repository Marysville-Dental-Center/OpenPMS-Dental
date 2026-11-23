"""GraphScheduleDay on the grid (UI-12 slice 4, layout script on the planner's tree; the schedule graph itself, panel1 and panel2, is
owner-drawn and stays). The planner left the Employees check box 76 px wide for its 81 px text, so it wrapped onto two lines in a 40 px
row, and the Filter check boxes 10 px under the Sort radio buttons and the Scale fields: the three option groups on one row 8 px apart from
the content edge (Sort, Filter, Scale), each as wide as its row; every item on the row under the title band (centred on the 23 px field
row), 6 px apart, the two hour pairs 12 px apart. Fix round: the groups as tall as their one row (8 px under it; the planner left them
71 px tall, 26 px empty under the row), the graph's panels moving up by as much (the host docks the control)."""


def layout(h):
    H = h.H
    x = 8
    for g, items in (("groupBox1", ("radioStartTime", "radioStopTime", "radioName")),
            ("groupBox2", ("checkBoxNotes", "checkBoxProviders", "checkBoxEmployees"))):
        grp = h.n(g)
        top = grp.title_bottom + 4
        cx = 8
        for name in items:
            h.check_left(name)
            h.set(name, cx, top + (H - 20) // 2, h.cw(name), 20)
            cx += h.cw(name) + 6
        h.set(g, x, None, cx - 6 + 8, None)
        x += cx - 6 + 8 + 8
    grp = h.n("groupBox3")
    top = grp.title_bottom + 4
    cx = 8
    for lab, fld, gap in (("label2", "numStartHour", 12), ("label3", "numEndHour", 8)):
        lw = h.lw(lab)
        h.set(lab, cx, top, lw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, cx + lw + 6, top, 44, H)
        cx += lw + 6 + 44 + gap
    h.set("groupBox3", x, None, cx, None)
    gh = top + H + 8
    for name in ("groupBox1", "groupBox2", "groupBox3"):
        h.set(name, None, None, None, gh)
    p1 = h.n("panel1")
    dy = 8 + gh + 8 - p1.ny
    for name in ("panel1", "panel2"):
        h.set(name, None, h.n(name).ny + dy, None, None)
    h.set(".", None, None, None, h.tree.h + dy)
