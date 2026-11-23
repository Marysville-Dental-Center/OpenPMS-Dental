"""FormReferralProcTrack on the grid (UI-12 slice 4, layout script on the planner's tree). Print stood 60 px over the window's bottom: the
grid down to 4 px over Print, Print 8 px over the bottom."""


def layout(h):
    root = h.tree
    y = root.h - 8 - 24
    h.set("butPrint", None, y)
    g = h.n("gridMain")
    h.set("gridMain", None, None, None, y - 4 - g.ny)
