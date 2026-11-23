"""FormScheduledProcesses on the grid (UI-12 slice 4, layout script on the planner's tree). The grid stood at x 30 and 37 px over the
window's bottom (the window has no bottom bar): the grid from the content edge to 8 px before Add and the group, down to the content's
bottom."""


def layout(h):
    root = h.tree
    a = h.n("butAdd")
    h.set("gridMain", 8, 8, a.nx - 8 - 8, root.h - 16)
