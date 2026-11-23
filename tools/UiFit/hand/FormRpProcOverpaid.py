"""FormRpProcOverpaid on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The planner kept the window's frame as drawn (the
filter group and the grid at main's 12, Refresh apart from the group, the grid short of the content edge): the script puts the filter group
at the content edge with Refresh 8 px after it on its top, the grid 8 px under the group from the content edge to the right content edge and
down to 8 px over the bottom bar, the bottom bar's buttons at the bar's height (24) 8 px from the bottom."""


def layout(h):
    root = h.tree
    R = root.w - 8
    g = h.n("groupBox2")
    h.set("groupBox2", 8, 8)
    h.set("butRefresh", 8 + g.w + 8, 8, 75, 24)
    yb = root.h - 8 - 24
    yg = 8 + g.h + 8
    h.set("gridMain", 8, yg, R - 8, yb - 8 - yg)
    for name in ("butPrint", "butFix"):
        if name in h.by_name:
            b = h.n(name)
            h.set(name, 8 if name == "butPrint" else b.x, yb, b.w, 24)
    # Fix round 8: the Clinics picker's box on the Providers column above it (its left and right edges; the planner left it 9 px left):
    # its bounds start its measured label area before the column.
    pv = h.n("comboBoxMultiProv")
    cw = h.clinic_w("comboBoxMultiClinics")
    h.set("comboBoxMultiClinics", pv.nx - cw, None, pv.nw + cw, None)
