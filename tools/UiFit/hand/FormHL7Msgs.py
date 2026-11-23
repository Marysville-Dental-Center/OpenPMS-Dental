"""FormHL7Msgs on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The planner has no fixed point here (the status
combo 2 px wider and Refresh 2 px further right on every pass): both kept at the planner's first pass (156 px; Refresh 6 px after the combo)."""


def layout(h):
    c = h.n("comboHL7Status")
    h.set("comboHL7Status", None, None, c.nw, None)
    h.set("butRefresh", c.nx + c.nw + 6, None, None, None)
