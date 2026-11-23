"""FormEhrLabResultEdit2014 (EHR Lab Result) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The planner
lays the window out but keeps the Coded Elements and Unit of Measure groups' first rows as drawn (their picker button stands over the first
label): the script lays out those two groups' rows like the groups around them, the picker button on the first row before the labels, at the
place and size the planner gave the groups."""


def layout(h):
    for gname, button in (("groupCE", "butCodedElementSnomed"), ("groupUnitsOfMeasure", "butUnitOfMeasureUCUM")):
        g = h.n(gname)
        b, _ = h.group_rows(gname, g.x, g.y, g.w, button=button)
        b.end()
    # The window's left content edge at 8 (the planner kept main's x).
    h.frame_left()
    # Fix round 8: the columns 8 px apart (frame_left left 6 between the left and middle columns), the Performing Organization column to the
    # window's content edge (Save's right edge): its groups, their nested groups and the fields that end on their content edge grow with it.
    g6, g7, go = h.n("groupBox6"), h.n("groupBox7"), h.n("groupOrganization")
    x7 = g6.nx + g6.nw + 8
    h.set("groupBox7", x7, None)
    h.set("butAddNote", x7, None)
    xo = x7 + g7.nw + 8
    R = h.tree.nw - 8
    h.set("groupOrganization", xo, None)
    grow(h, go, R - xo - go.nw)
    # The observation value box on its column's right edge (1 px past it).
    h.set("textObsValue", None, None, h.n("textObsDateTime").nw, None)


def grow(h, c, dw):
    """Widens container c by dw; its group boxes and the fields ending on its content edge (within 2 px) grow with it."""
    edge = c.nw - 8
    h.set(c.name, None, None, c.nw + dw, None)
    for k in c.children:
        if not k.name or k.dock not in ("None", ""):
            continue
        if k.kind == "group" and abs(k.nx + k.nw - edge) <= 2:
            grow(h, k, dw)
        elif k.kind == "field" and abs(k.nx + k.nw - edge) <= 2:
            h.set(k.name, None, None, k.nw + dw, None)
