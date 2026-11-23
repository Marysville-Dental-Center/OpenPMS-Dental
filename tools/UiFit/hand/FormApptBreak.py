"""FormApptBreak on the grid (UI-12 slice 8, layout script on the planner's tree). The three action buttons stood at x 29, 143 px wide and
23 tall under a 190 px group (neither centred nor on its edges): the buttons as wide as the group, from the padding, 24 px tall on a 4 px
gap, 8 px under the group; the last one ends on the bottom padding (main's 155 px height)."""


def layout(h):
    g = h.n("groupBox1")
    y = g.ny + g.nh + 8
    for name in ("butUnsched", "butPinboard", "butApptBook"):
        h.set(name, g.nx, y, g.nw, 24)
        y += 24 + 4
