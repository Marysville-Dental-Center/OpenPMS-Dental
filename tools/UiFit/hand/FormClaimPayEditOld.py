"""FormClaimPayEditOld on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). Delete and its note are anchored to the
bottom (Bottom|Right): the planner read LayoutManager's reset anchor in the tree and put them under Show Unattached, where a shorter window
ran Delete into the check box. Both at the bottom of the column as on main: the note (two lines) ending on the grid's bottom edge, Delete
4 px over it."""


def layout(h):
    g = h.n("gridMain")
    bottom = g.ny + g.nh
    note = h.n("label1")
    yn = bottom - note.nh
    h.set("label1", None, yn, None, None)
    h.set("butDelete", None, yn - 4 - 24, None, None)
