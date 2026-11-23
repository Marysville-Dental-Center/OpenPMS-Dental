"""UserControlSetupWizSchedule (the setup wizard's Schedules page) on the grid (UI-12 slice 2 continuation, mode hand). The page's own
controls only; SetupWizControl's title and description labels are its base class's designer, which the program never runs (the base has no
constructor that calls InitializeComponent; the harness builds the page the same way since this slice: WindowBuilder.CreateBare).
The provider line at the content edge with Prev Prov and Next Prov 6 px apart at the right content edge; the instruction for Next Prov under
it; the seven day groups 8 px under them, one width and one height: Monday to Thursday on the content's four columns 8 px apart, Friday to
Sunday on one row under them, centred (half a column in); each day's check box stands on its group's title band at the group's padding (the
code shows the group when the box is checked: UserControlSetupWizSchedule.cs), the grid inside to the content edges, Add at the content's
bottom left. The groups fill the page's height."""


def layout(h):
    H = h.H
    CH = h.CH
    root = h.tree
    W, HH = root.w, root.h
    R = W - 8
    # The provider line and the buttons.
    h.set("butNextProv", R - 75, 8, 75, 24)
    h.set("butPrevProv", R - 75 - 6 - 75, 8, 75, 24)
    wp = R - 75 - 6 - 75 - 16 - 8
    h.set("labelProv", 8, 8, min(wp, max(h.lw("labelProv"), 400)), 24)
    h.text_align("labelProv", "MiddleLeft")
    h.set("label2", 8, 8 + 24 + 4, h.lw("label2"), H)
    h.text_align("label2", "MiddleLeft")
    yg = 8 + 24 + 4 + H + 8
    gw = (R - 8 - 3 * 8) // 4
    gh = (HH - 8 - yg - 8) // 2
    days = (("groupMon", "checkMonday", "gridMonday", "butAddMonday"), ("groupTue", "checkTuesday", "gridTuesday", "butAddTuesday"),
            ("groupWed", "checkWednesday", "gridWednesday", "butAddWednesday"), ("groupThu", "checkThursday", "gridThursday", "butAddThursday"),
            ("groupFri", "checkFriday", "gridFriday", "butAddFriday"), ("groupSat", "checkSaturday", "gridSaturday", "butAddSaturday"),
            ("groupSun", "checkSunday", "gridSunday", "butAddSunday"))
    top = 18 + 4  # the title band of a group box with a title (the check box stands on it)
    for i, (grp, chk, grid, add) in enumerate(days):
        if i < 4:
            x = 8 + i * (gw + 8)
            y = yg
        else:
            x = 8 + (gw + 8) // 2 + (i - 4) * (gw + 8)
            y = yg + gh + 8
        h.set(grp, x, y, gw, gh)
        h.check_left(chk)
        h.set(chk, x + 8, y + (top - 4 - CH) // 2, h.cw(chk), CH)
        yb = gh - 8 - 24
        h.set(add, 8, yb, 75, 24)
        h.set(grid, 8, top, gw - 16, yb - 8 - top)
        h.watch(grp)
