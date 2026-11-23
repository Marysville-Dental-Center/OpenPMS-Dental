"""FormRpOutstandingIns (Outstanding Insurance Claims) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The
planner gave the filter group's last column two left edges (Last Error Definition's combo box 23 px right of the other two, so that its long
label clears the Mine button of the For User row): the script puts the column's three fields on one x, 10 px after Mine plus the label (1000)
and on the group's content edge, each label right-aligned 6 px before."""


def layout(h):
    mine = h.n("butMine")
    right = h.n("comboLastClaimTrack").x + h.n("comboLastClaimTrack").w
    lw3 = h.lw("label3")
    x = max(h.n("comboLastClaimTrack").x, mine.x + mine.w + 10 + lw3 + 6)
    for lab, fld in (("label4", "comboLastClaimTrack"), ("labelCarrier", "textCarrier"), ("label3", "comboErrorDef")):
        h.set(fld, x, None, right - x)
        lw = h.lw(lab)
        h.set(lab, x - 6 - lw, None, lw)
        h.text_align(lab, "MiddleRight")
    # The window's left content edge at 8 (the planner kept main's x).
    dx = h.frame_left()
    # The groups beside the bottom buttons move left with them (their fields keep their distance to the filter group's columns).
    for name in ("groupBox1", "groupBox3", "groupBoxUpdateCustomTracking"):
        h.set(name, h.n(name).x - dx, None)
    # Fix round 8: the three groups stay where frame_left put them (their fields on the filter group's columns); Assign User and Zero Claims
    # narrow to their text (68 and 69 px) so that Zero Claims stands 6 px after Assign User and 8 px before the groups (frame_left moved the
    # groups but not Zero Claims: they touched). The Custom Tracking group as tall as the two info groups beside it (one height for groups
    # side by side).
    au = h.n("butAssignUser")
    xg = h.n("groupBox1").nx
    wz = 72
    xz = xg - 8 - wz
    h.set("butAssignUser", None, None, xz - 6 - au.nx, None)
    h.set("butZeroClaims", xz, None, wz, None)
    h.set("groupBoxUpdateCustomTracking", None, None, None, h.n("groupBox1").nh)
