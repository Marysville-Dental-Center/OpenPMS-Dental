"""FormInsPlans on the grid (UI-12 slice 3, layout script of grid_hand.py on the planner's tree; fix round). The planner left Get All
and Show Hidden 4 px after the Order By group (FormInsPlansOverrides, the twin window: 10 px): both 8 px after it, as in
FormInsPlansOverrides."""


def layout(h):
    o = h.n("groupBox2")
    for name in ("butGetAll", "checkShowHidden"):
        h.set(name, o.nx + o.nw + 8, None)
