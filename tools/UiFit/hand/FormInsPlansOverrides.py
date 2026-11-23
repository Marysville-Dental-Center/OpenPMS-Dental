"""FormInsPlansOverrides on the grid (UI-12 slice 3, layout script of grid_hand.py on the planner's tree). The planner left the Overrides
group 6 px after the grid and 7 px short of the right content edge (the Filter group's), and Select All 3 px past the grid's right edge:
the group 8 px after the grid to the right content edge (its inner group to its content edge), Select All ending on the grid's edge.
Get All and Show Hidden 8 px after the Order By group, as in FormInsPlans (the planner left them 10 px after it here, 4 px there)."""


def layout(h):
    R = h.tree.w - 8
    g = h.n("gridMain")
    gx = g.nx + g.nw + 8
    h.set("groupOverrides", gx, None, R - gx, None)
    inner = h.n("groupNoBillInsOverrides")
    h.set("groupNoBillInsOverrides", None, None, (R - gx) - 8 - inner.nx, None)
    for lab in ("labelNoBillIns", "labelBillToIns", "labelDeleteNoBillIns"):
        n = h.n(lab)
        h.set(lab, None, None, ((R - gx) - 8 - inner.nx) - 8 - n.nx, None)
    s = h.n("butSelectAll")
    h.set("butSelectAll", g.nx + g.nw - s.nw, None)
    o = h.n("groupBox2")
    for name in ("butGetAll", "checkShowHidden"):
        h.set(name, o.nx + o.nw + 8, None)
