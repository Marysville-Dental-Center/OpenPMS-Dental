"""FormReportSetup (Report Setup) on the grid (UI-12 slice 1 fix round 8, layout script of grid_hand.py, on the planner's tree). The planner
grew the window (its Misc Options page) and kept UserControlReportSetup, the Display Settings page's content, at its designer's 525 px, so its
grid and Up/Down buttons ended 29 px short of the page's right side (13 px on the left). The user control (only this window shows it; its
code moves it to the Security Permissions page and back) now anchors its grid to its four sides and its Up/Down buttons and allowed count to
its top right (UserControlReportSetup.Designer.cs); the script sizes it to the page less its own 3 px margin, so its content stands 8 px
from the page's edges on every side."""


def layout(h):
    # The page as the tab control sizes it (its client less the 2 px border and the 21 px tab row; the designer's page size is stale).
    tc = h.n("tabControl1")
    pw, ph = tc.nw - 4, tc.nh - 23
    h.set("userControlReportSetup", 5, 5, pw - 10, ph - 10)
