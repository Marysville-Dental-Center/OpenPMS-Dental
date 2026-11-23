"""FormTaskSearch on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree). The planner left the search panel's
fields on one left edge but at four right edges (178, 189, 190 and 200 in the panel). Every field of the panel ends on one edge, the nested
groups' content edge (8 px inside them): the picker buttons ("...", 24 px) at that edge with their fields 6 px before them, the date pickers'
Clear buttons right-aligned on it."""


def layout(h):
    H = h.H
    g2 = h.n("groupBox2")
    E = g2.nw - 8 - 8  # the nested groups' content edge, in the panel's coordinates
    Eg = E - 8  # the same edge in a nested group's coordinates
    for name in ("textTaskList", "textTaskNum", "comboPriority"):
        n = h.n(name)
        h.set(name, None, None, E - n.nx, H)
    p = h.n("textPatNum")
    h.set("butPatPicker", E - 24, p.ny, 24, H)
    h.set("textPatNum", None, None, E - 24 - 6 - p.nx, H)
    u = h.n("comboUsers")
    h.set("butUserPicker", Eg - 24, u.ny, 24, H)
    h.set("comboUsers", None, None, Eg - 24 - 6 - u.nx, H)
    for name in ("textIncluding", "textExcluding", "dateCreatedFrom", "dateCreatedTo", "dateCompletedFrom", "dateCompletedTo"):
        n = h.n(name)
        h.set(name, None, None, Eg - n.nx, H)
    for name in ("butClearCreated", "butClearCompleted"):
        b = h.n(name)
        h.set(name, Eg - b.nw, None, None, None)
