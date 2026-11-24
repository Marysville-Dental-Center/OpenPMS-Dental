"""ODCodeRangeFilter (a shared control) on the grid (UI-12 slice 16): the example line starts on the box's left edge (it stood 2 px in)
under the box; the control keeps the 37 px its hosts give it."""

KEEP_BAR_GAP = True


def layout(h):
    e = h.n("labelExample")
    h.set("labelExample", 0, e.ny)
