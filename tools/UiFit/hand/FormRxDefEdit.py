"""FormRxDefEdit on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). One touch on the planner's layout: Delete at the
bottom bar's left content edge (8 px from the window's left, as Save is 8 px from its right; the planner kept main's x)."""


def layout(h):
    h.set("butDelete", 8, None)
