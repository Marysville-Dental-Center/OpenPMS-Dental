"""FormCarriers on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). Add and Combine are anchored to the bottom
(Bottom|Right): the planner read LayoutManager's reset anchor in the tree and put them at the top of the side column, where a taller window
pushed Combine onto the ITRANS group (Top|Right, shown for a Canadian clearinghouse). Both at the bottom of the column as on main, Combine
on the grid's bottom edge, Add one button row (4 px) over it."""


def layout(h):
    g = h.n("gridMain")
    bottom = g.ny + g.nh
    yc = bottom - 24
    h.set("butCombine", None, yc, None, None)
    h.set("butAdd", None, yc - 4 - 24, None, None)
