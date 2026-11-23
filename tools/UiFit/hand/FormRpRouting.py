"""FormRpRouting on the grid (UI-12 slice 4, layout script on the planner's tree). The planner put the two All boxes on one row 100 px
right of their lists and Today and Displayed on their own at the right edge: the Providers and Clinics columns from the content edge, 8 px
apart (the caption, its All box 2 px under it, the list 2 px under the box; Clinics as wide as its All (Includes hidden) box); the Date row
8 px under the lists, its box on the Clinics column with Today and Displayed 6 px after it (they set that date); OK at the bottom right."""


def layout(h):
    H = h.H
    CH = h.CH
    yl = 8 + 18 + 2 + CH + 2
    lst = h.n("listProv")
    ybot = yl + 186
    x = 8
    for cap, chk, l, w in (("label1", "checkProvAll", "listProv", 130), ("labelClin", "checkClinAll", "listClin", 140)):
        h.set(cap, x, 8, w, 18)
        h.text_align(cap, "BottomLeft")
        h.check_left(chk)
        h.set(chk, x, 8 + 18 + 2, h.cw(chk), CH)
        h.set(l, x, yl, w, ybot - yl)
        xc = x
        x += w + 8
    y = ybot + 8
    h.set("textDate", xc, y, 80, H)
    lw = h.lw("label2")
    h.set("label2", xc - 6 - lw, y, lw, H)
    h.text_align("label2", "MiddleRight")
    h.set("butToday", xc + 80 + 6, y, 96, H)
    h.set("butDisplayed", xc + 80 + 6 + 96 + 6, y, 96, H)
