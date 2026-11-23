"""FormClaimPayEdit (Insurance Payment) on the grid (UI-12 slice 1 fix round 8, layout script of grid_hand.py, on the planner's tree). The
planner ended the right column on three edges: the carrier's Pick button, the Note box 14 px short of it and the Virtual Credit Card Payment
group 4 px short (main ends all three on one edge). The script ends the Note box and the group on Pick's right edge; the group's buttons stay
where the planner put them, from its left padding 6 px apart (the code shows some of them; the group shows when one is shown)."""


def layout(h):
    pk = h.n("butCarrierSelect")
    R = pk.nx + pk.nw
    for name in ("textNote", "groupPrepaid"):
        n = h.n(name)
        h.set(name, None, None, R - n.nx, None)
