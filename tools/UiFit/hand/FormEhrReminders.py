"""FormEhrReminders on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). Edit ended 7 px past the grids, which stood
12 px from the left edge and 15 px from the right: both grids 12 px from each side, Edit right-aligned on their right edge, the contact box
6 px before Edit."""


def layout(h):
    root = h.tree
    R = root.w - 12
    for name in ("gridMain", "gridProvided"):
        g = h.n(name)
        h.set(name, None, None, R - g.nx, None)
    e = h.n("butEdit")
    xe = R - e.nw
    h.set("butEdit", xe, None, None, None)
    t = h.n("textPreferedConfidentialContact")
    h.set("textPreferedConfidentialContact", None, None, xe - 6 - t.nx, None)
