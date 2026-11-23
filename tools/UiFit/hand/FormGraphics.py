"""FormGraphics on the grid (UI-12 slice 2, mode hand: the planner left two of This Computer's radio buttons over one another and the
window at main's 27/28 px margin). The intro at the content edge; the All Computers group under it as wide as the first two groups of the
next row; the This Computer, Status and Older Technology groups side by side 8 px apart, one height, the last to the right content edge;
the Options and Preview groups side by side, one height, Preview at the right content edge; in each group its radio buttons and check box a
list at the content edge on the 22 px pitch (a two-line one as tall as its lines), each note under the button it explains, 16 px in; the
window grows by what the rows need; Save at the bottom right."""


def layout(h):
    CH = h.CH
    root = h.tree
    R = root.w - 8
    y = 8
    h.set("labelIntro", 8, y, R - 8, 36)
    y += 36 + 8
    G1, G2 = 180, 360
    x2 = 8 + G1 + 8
    x3 = x2 + G2 + 8
    G3 = R - x3

    def radio(name, x, yy, w=None, hh=None):
        h.check_left(name)
        h.set(name, x, yy, w if w else h.cw(name), hh if hh else CH)

    # All computers
    top = 22
    wa = G1 + 8 + G2
    h.set("labelChart3DAll", 8, top, wa - 16, 18)
    radio("radioChart3DAllUse", 8, top + 22)
    radio("radioChart3DAllDontUse", 8, top + 44, wa - 16, 34)
    ha = top + 44 + 34 + 8
    h.set("groupChart3DAll", 8, y, wa, ha)
    y += ha + 8
    # This computer, status, older technology
    radio("radioChart3DThisGlobal", 8, top)
    radio("radioChart3DThisUse", 8, top + 22)
    radio("radioChart3DThisDontUse", 8, top + 44)
    radio("radioStatusAvailable", 8, top)
    radio("radioStatusFellBack", 8, top + 22, G2 - 16, 36)
    radio("radioSimpleChart", 8, top)
    h.set("labelOlder", 24, top + 22, G3 - 32, 32)
    hr = max(top + 44 + CH, top + 22 + 36, top + 22 + 32) + 8
    h.set("groupChart3DThis", 8, y, G1, hr)
    h.set("groupStatus", x2, y, G2, hr)
    h.set("groupOlder", x3, y, G3, hr)
    y += hr + 8
    # Options and preview
    PW = 410 + 16
    px = R - PW
    h.set("toothChartWrapper", 8, top, 410, 307)
    h.set("labelPreviewStatus", 8, top + 307 + 4, 410, 30)
    h.set("labelPreview", 8, top + 307 + 4 + 30 + 4, 410, 18)
    hp = top + 307 + 4 + 30 + 4 + 18 + 8
    h.set("groupPreview", px, y, PW, hp)
    ow = px - 8 - 8
    h.set("labelOptions", 8, top, ow - 16, 32)
    radio("checkDrawInSoftware", 8, top + 32 + 4)
    h.set("labelDrawInSoftware", 24, top + 32 + 4 + CH + 4, ow - 32, 70)
    h.set("groupOptions", 8, y, ow, hp)
    for g in ("groupChart3DAll", "groupChart3DThis", "groupStatus", "groupOlder", "groupPreview", "groupOptions"):
        h.watch(g)
    y += hp + 8
    hh = max(root.h, y + 26 + 8)
    h.set("butSave", R - 75, hh - 8 - 26, 75, 26)
    h.set(".", None, None, root.w, hh)
