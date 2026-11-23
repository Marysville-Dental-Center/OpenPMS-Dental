"""FormRpLaserLabels (Laser Labels) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The Patient Labels
page holds a second "End Name:" label that main keeps outside the page (x -95: never seen, the page shows "End Name (L, F):" beside the box);
the planner brought it into the page beside the visible label. The script keeps it where main has it."""


def layout(h):
    h.set("labEndName", -95, 53, 61, 20)
    # The window's left content edge at 8 (the planner kept main's x).
    h.frame_left()
