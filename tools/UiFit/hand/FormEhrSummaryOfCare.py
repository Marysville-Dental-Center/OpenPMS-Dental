"""FormEhrSummaryOfCare on the grid (UI-12 slice 4, layout script on the planner's tree). The planner kept main's left column at x 20 and
the send group's note 192 px wide, which cut its third line ("ccd.xsl - for human readable viewing", 201 px): the left column (the send
and show groups, the sent grid, Delete) from the content edge, 220 px wide (the note's widest line + the group's padding); the right column
(the receive group and the received grid) 8 px after it to the right content edge."""


def layout(h):
    R = h.tree.w - 8
    wl = 220
    for name in ("groupBox3", "groupBox1", "gridSent", "butDelete"):
        n = h.n(name)
        h.set(name, 8, None, wl if name != "butDelete" else None, None)
    h.set("label1", 8, None, wl - 16, None)
    xr = 8 + wl + 8
    for name in ("groupBox2", "gridRec"):
        h.set(name, xr, None, R - xr, None)
