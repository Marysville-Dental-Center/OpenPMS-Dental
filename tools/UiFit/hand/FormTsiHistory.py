"""FormTsiHistory on the grid (UI-12 slice 3, layout script of grid_hand.py on the planner's tree). The planner left the clinic picker's
box 12 px short of the Client IDs box under it (both on one field column): the picker's box ends on that box's right edge."""


def layout(h):
    p = h.n("comboClinics")
    c = h.n("comboClientIDs")
    h.set("comboClinics", None, None, c.nx + c.nw - p.nx, None)
