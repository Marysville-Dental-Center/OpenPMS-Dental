"""FormCloneAdd (Clone Patient) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The Specialty and Clinic rows
are hidden in the designer and shown by Load (FormCloneAdd.cs: Specialty always, Clinic with clinics on). The planner moves hidden rows to the
end of their column and put Clone on the row after Primary Provider, so with the rows shown Clone stood between Primary Provider and
Specialty. The script puts the two rows under Primary Provider in main's order on the row pitch and Clone at the window's bottom right, 8 px
from the edges (as on main); the window keeps the planner's height, which holds the shown rows."""


def layout(h):
    H = h.H
    root = h.tree
    prov = h.n("comboPriProv")
    # Fix round 8: Primary Provider on the patient group's field column and its right edge (the fields' widths are the planner's fixed point,
    # pinned in grid-hand.tsv: 240 and 90 px, ending 240 px after the column), its picker button 6 px after it, as FormAdjust's Provider row.
    g = h.n("groupBox1")
    R = g.nx + h.n("textLName").nx + 240
    h.set("comboPriProv", None, None, R - prov.nx, None)
    bp = h.n("butPickPrimary")
    h.set("butPickPrimary", R + 6, None)
    y = prov.y + H + 4
    for label, field in (("labelSpecialty", "comboSpecialty"), ("labelClinic", "comboClinic")):
        lb = h.n(label)
        h.set(field, prov.x, y, prov.nw, H)
        h.set(label, prov.x - 6 - lb.w, y, lb.w, H)
        y += H + 4
    b = h.n("butClone")
    h.set("butClone", root.w - 8 - b.w, root.h - 8 - 24, b.w, 24)
