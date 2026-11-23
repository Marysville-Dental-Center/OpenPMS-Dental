"""FormRpReceivablesBreakdown on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). A month calendar takes 190 x 176 px
when its handle is made: the Insurance Writeoffs group (270 px) ran 6 px under the calendar's drawn left edge. The group ends on the Clinic
list's right edge, 6 px before the calendar."""


def layout(h):
    lst = h.n("listClinic")
    g = h.n("groupInsBox")
    h.set("groupInsBox", None, None, lst.nx + lst.nw - g.nx, None)
