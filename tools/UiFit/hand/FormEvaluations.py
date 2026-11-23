"""FormEvaluations on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The planner has no fixed point here (the
Course combo and the To date 2 px wider on every pass) and left the grid's height as drawn when it moved it to the top (41 px under it
against 8 over it): the grid 12 px over the bottom (main's 13; 12 from the left edge); the Date row and the Course combo end on the right
edge of the filter groups' fields, Refresh, Add and Reports; the two date boxes one width with "to" between them, 6 px apart."""


def layout(h):
    g = h.n("gridMain")
    root = h.tree
    h.set("gridMain", None, None, None, root.h - 12 - g.ny)
    add = h.n("butAdd")
    R = add.nx + add.nw
    c = h.n("comboCourse")
    h.set("comboCourse", None, None, R - c.nx, None)
    s = h.n("textDateStart")
    to = h.n("label6")
    w = (R - s.nx - 6 - to.nw - 6) // 2
    h.set("textDateStart", None, None, w, None)
    h.set("label6", s.nx + w + 6, None, None, None)
    h.set("textDateEnd", R - w, None, w, None)
