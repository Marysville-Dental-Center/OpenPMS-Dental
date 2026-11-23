"""FormTreatPlanEdit on the grid (UI-12 slice 5, layout script on the planner's tree). Heading and Note ended 26 px short of Save's edge
(their column's own edge): both to the right content edge."""


def layout(h):
    R = h.tree.nw - 8
    for name in ("textHeading", "textNote"):
        n = h.n(name)
        h.set(name, None, None, R - n.nx, None)
