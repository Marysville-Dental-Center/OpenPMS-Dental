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
    # Fix round 8: the top row is as tall as the day label's two lines (Load and the arrows set it to the day of the week over the short date,
    # grid-runtime-texts.tsv), the label centred between the arrows on the whole span (main: MiddleCenter), the row's 23 px controls centred
    # on it.
    TR = 32
    y = 8
    yc = y + (TR - H) // 2
    h.set("butBack", 8, yc, 39, H)
    fw = h.n("butForward")
    xd = 8 + 39 + 6
    xf = fw.nx
    h.set("labelDate", xd, y, xf - 6 - xd, TR)
    h.text_align("labelDate", "MiddleCenter")
    h.set("butForward", None, yc, 39, H)
    h.set("comboClinic", R - 217, yc, 217, H)
    ws = 180
    xs = R - 217 - 16 - ws
    ls = h.lw("labelSearch")
    h.set("textSearch", xs, yc, ws, H)
    h.set("labelSearch", xs - 6 - ls, yc, ls, H)
    h.text_align("labelSearch", "MiddleRight")
    yt = y + TR + 8
    yb = HW - 8 - 24
    g3 = h.n("groupBox3")
    xg = R - g3.w
    h.set("groupBox3", xg, yt, g3.w, g3.h)
    h.set("tabControl1", 8, yt, xg - 8 - 8, yb - 8 - yt)
    # Fix round 8: the group's content 8 px inside it on both sides (main's 176 px content in the 193 px group left 17 px on the right): the
    # provider/employee tabs to the content edge, Add and Note on one row of two halves 7 px apart (the pilot's button gap in a pair).
    CW = g3.w - 16
    BW = (CW - 7) // 2
    h.set("tabControl2", 8, None, CW, None)
    h.set("butAddTime", 8, None, BW, None)
    h.set("butProvNote", 8 + CW - BW, None, BW, None)
    yd = yt + g3.h + 8
    h.set("butCloseOffice", xg, yd, 80, 24)
    gp = h.box("groupPractice", xg, yd + 24 + 8, g3.w)
    gp.row(("butNote", gp.left, BW, 24), ("butHoliday", gp.left + CW - BW, BW, 24))
    gp.end()
    h.set("butSave", R - 75, yb, 75, 24)
    h.set("butOkSchedule", R - 75 - 6 - 122, yb, 122, 24)
