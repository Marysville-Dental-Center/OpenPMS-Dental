"""FormCloneAdd (Clone Patient) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The Specialty and Clinic rows
are hidden in the designer and shown by Load (FormCloneAdd.cs: Specialty always, Clinic with clinics on). The planner moves hidden rows to the
end of their column and put Clone on the row after Primary Provider, so with the rows shown Clone stood between Primary Provider and
Specialty. The script puts the two rows under Primary Provider in main's order on the row pitch and Clone at the window's bottom right, 8 px
from the edges (as on main); the window keeps the planner's height, which holds the shown rows."""


def layout(h):
    H = h.H
    root = h.tree
    prov = h.n("comboPriProv")
    y = prov.y + H + 4
    for label, field in (("labelSpecialty", "comboSpecialty"), ("labelClinic", "comboClinic")):
        lb = h.n(label)
        h.set(field, prov.x, y, prov.w, H)
        h.set(label, prov.x - 6 - lb.w, y, lb.w, H)
        y += H + 4
    b = h.n("butClone")
    h.set("butClone", root.w - 8 - b.w, root.h - 8 - 24, b.w, 24)
