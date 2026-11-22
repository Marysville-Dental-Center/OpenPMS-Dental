"""FormScheduleDayEdit (Edit Day) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The planner leaves most
of the window as drawn (a tab control of one grid and one graph, a group of lists): the script puts the top row on one line (the day arrows
and the date, Search, the clinic picker ending at the content edge), the tab control at the content edge from 8 px under the top row to 8 px
over the bottom bar, the Add Time Block group at the right on the tab control's top, Delete and the Practice group (its two buttons on one
row) under it on its edges, the bottom bar 8 px from the edges. The graph page is GraphScheduleDay, a user control with its own designer."""


def layout(h):
    H = h.H
    root = h.tree
    # GraphScheduleDay (another designer) holds a groupBox3 of its own: the script's is the window's Add Time Block group.
    h.by_name["groupBox3"] = [k for k in root.children if k.name == "groupBox3"][0]
    WW, HW = root.w, root.h
    R = WW - 8
    y = 8
    h.set("butBack", 8, y, 39, H)
    fw = h.n("butForward")
    ld = h.n("labelDate")
    h.set("labelDate", None, y, None, H)
    h.set("butForward", None, y, 39, H)
    h.set("comboClinic", R - 217, y, 217, H)
    ws = 180
    xs = R - 217 - 16 - ws
    ls = h.lw("labelSearch")
    h.set("textSearch", xs, y, ws, H)
    h.set("labelSearch", xs - 6 - ls, y, ls, H)
    h.text_align("labelSearch", "MiddleRight")
    yt = y + H + 8
    yb = HW - 8 - 24
    g3 = h.n("groupBox3")
    xg = R - g3.w
    h.set("groupBox3", xg, yt, g3.w, g3.h)
    h.set("tabControl1", 8, yt, xg - 8 - 8, yb - 8 - yt)
    yd = yt + g3.h + 8
    h.set("butCloseOffice", xg, yd, 80, 24)
    gp = h.box("groupPractice", xg, yd + 24 + 8, g3.w)
    gp.row(("butNote", gp.left, 80, 24), ("butHoliday", None, 80, 24))
    gp.end()
    h.set("butSave", R - 75, yb, 75, 24)
    h.set("butOkSchedule", R - 75 - 6 - 122, yb, 122, 24)
