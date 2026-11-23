"""FormEhrLabPanels on the grid (UI-12 slice 4, layout script on the planner's tree). The planner kept main's left column at 21 while OK
stands at the right content edge: Add Panel, the grid and the HL7 Msg group at the content edge, the grid to the right content edge."""


def layout(h):
    R = h.tree.w - 8
    h.set("butAdd", 8, None)
    h.set("gridMain", 8, None, R - 8, None)
    h.set("groupHL7", 8, None)
