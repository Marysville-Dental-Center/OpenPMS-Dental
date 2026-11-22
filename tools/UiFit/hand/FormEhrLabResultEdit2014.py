"""FormEhrLabResultEdit2014 (EHR Lab Result) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The planner
lays the window out but keeps the Coded Elements and Unit of Measure groups' first rows as drawn (their picker button stands over the first
label): the script lays out those two groups' rows like the groups around them, the picker button on the first row before the labels, at the
place and size the planner gave the groups."""


def layout(h):
    for gname, button in (("groupCE", "butCodedElementSnomed"), ("groupUnitsOfMeasure", "butUnitOfMeasureUCUM")):
        g = h.n(gname)
        b, _ = h.group_rows(gname, g.x, g.y, g.w, button=button)
        b.end()
