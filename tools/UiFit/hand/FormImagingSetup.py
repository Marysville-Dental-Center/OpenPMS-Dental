"""FormImagingSetup on the grid (UI-12 slice 3, layout script of grid_hand.py on the planner's tree). Set Default Scanner's text (111 px)
filled its 112 px button: the button is 124 px wide (6 px on each side of the text, snapped to the grid)."""


def layout(h):
    h.set("butSetScanner", None, None, 124, None)
