"""UserControlReportSetup on the grid (UI-12 slice 4, layout script on the planner's tree). The page shows one of two top rows (Load,
_isPermissionMode): the instructions with Up and Down, or the user group row (User Group, Set All, Set None) with the allowed count. The
planner moved both 5 px right and down at main's widths, so Down and the allowed count ended past the page's 525 px (cut in FormReportSetup
and FormCentralReportSetup). On one row at the content top: the instructions (two lines, 340 px for the longer one) with Up and Down at the
right content edge; the user group label 6 px before its combo, Set All and Set None 6 px apart, and the allowed count right-aligned on the
right content edge, as wide as its widest text ("Reports allowed: 999 / 999", grid-runtime-texts); the grid 8 px under the instructions
down to 8 px over the page's bottom (the hosts' 607 px; the page keeps main's size)."""


def layout(h):
    H = h.H
    W, Hh = 525, 607
    R = W - 8
    h.set(".", None, None, W, Hh)
    h.set("labelInstructions", 8, 8, 340, 31)
    h.set("butDown", R - 63, 8, 63, 24)
    h.set("butUp", R - 63 - 6 - 63, 8, 63, 24)
    lw = h.lw("labelUserGroup")
    h.set("labelUserGroup", 8, 8, lw, H)
    h.text_align("labelUserGroup", "MiddleRight")
    x = 8 + lw + 6
    h.set("comboUserGroup", x, 8, 133, H)
    x += 133 + 6
    h.set("butSetAll", x, 8, 66, H)
    x += 66 + 6
    h.set("butSetNone", x, 8, 66, H)
    wa = h.lw("labelAllowedCount")
    h.set("labelAllowedCount", R - wa, 8, wa, H)
    top = 8 + 31 + 8
    h.set("gridReports", 8, top, R - 8, Hh - 8 - top)
