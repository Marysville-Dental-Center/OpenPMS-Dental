"""ODCodeRangeFilter (a shared control) on the grid (UI-12 slice 16): the example line under the box, on the box's left edge (it stood
2 px in) and as wide as it, 14 px tall so it ends on the control's 37 px (the height its hosts give it; at 16 px it ran 2 px past it)."""

KEEP_BAR_GAP = True


def layout(h):
    h.set("labelExample", 0, 23, 150, 14)
