"""FormAdjustmentPicker on the grid (UI-12 slice 4, layout script on the planner's tree). The planner kept main's 12 px left margin with 12
at the right: the grid from the content edge to the right content edge, Show Unattached Only under it at the content edge."""


def layout(h):
    R = h.tree.w - 8
    h.set("gridMain", 8, None, R - 8, None)
    h.set("checkUnattached", 8, None)
