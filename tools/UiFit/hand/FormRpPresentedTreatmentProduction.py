"""FormRpPresentedTreatmentProduction on the grid (UI-12 slice 2, mode hand). A month calendar takes 190 x 176 px when its handle is made (the designer says 178 x 155;
slice 1, FormRpPaySheet), so main's captions and All boxes under the calendars stood on their Today line, covered; the planner kept them
there. The calendars side by side from the content edge, 8 px apart; under them (8 px under their drawn height) the Users and Clinics
columns (caption, All 2 px under it, the list 2 px under the box down to 8 px over the bottom bar) from the content edge, 8 px apart, and the
three option groups 8 px after them to the content's right edge, 8 px apart, each as tall as its two radio buttons (the window grows by
what they need at its font); OK at the bottom right."""


def layout(h):
    H = h.H
    CH = h.CH
    root = h.tree
    R = root.w - 8
    CAL_W, CAL_H = 190, 176
    h.set("date1", 8, 8, CAL_W, 155)
    h.set("date2", 8 + CAL_W + 8, 8, CAL_W, 155)
    y2 = 8 + CAL_H + 8
    # The groups first: the window grows by what they need (their radio buttons are 23 px at this window's font).
    gys = []
    y = y2
    for grp in ("groupType", "groupOrder", "groupUser"):
        top = (h.n(grp).title_bottom or 18) + 4
        gh = top + 2 * CH + 4 + 8
        gys.append((y, gh))
        y += gh + 8
    hr = max(root.h, y + 24 + 8)
    h.set(".", None, None, root.w, hr)
    yb = hr - 8 - 24
    GW = 185
    GX = R - GW
    room = GX - 8 - 8 - 8
    ws = [room // 2, room - room // 2]
    xs = [8, 8 + ws[0] + 8]
    for (cap, chk, lst), x, w in zip((("labelUsers", "checkAllUsers", "listUser"), ("labelClin", "checkAllClinics", "listClin")), xs, ws):
        h.set(cap, x, y2, w, 18)
        h.text_align(cap, "BottomLeft")
        h.check_left(chk)
        h.set(chk, x, y2 + 18 + 2, h.cw(chk), CH)
        yl = y2 + 18 + 2 + CH + 2
        h.set(lst, x, yl, w, yb - 8 - yl)
    for (y, gh), (grp, r1, r2) in zip(gys, (("groupType", "radioDetailed", "radioTotals"), ("groupOrder", "radioFirstPresented", "radioLastPresented"),
            ("groupUser", "radioPresenter", "radioEntryUser"))):
        top = (h.n(grp).title_bottom or 18) + 4
        for i, r in enumerate((r1, r2)):
            h.check_left(r)
            h.set(r, 8, top + i * (CH + 4), h.cw(r), CH)
        h.set(grp, GX, y, GW, gh)
        h.watch(grp)
    h.set("butOK", R - 75, yb, 75, 24)
