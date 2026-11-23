"""FormEhrPatListResults on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). Print is anchored to the bottom
(Bottom|Right): the planner read LayoutManager's reset anchor in the tree and put it under the Order By group. At the bottom of the side
column as on main, on the grid's bottom edge."""


def layout(h):
    g = h.n("gridMain")
    p = h.n("butPrint")
    h.set("butPrint", None, g.ny + g.nh - p.nh, None, None)
