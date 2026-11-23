"""FormClearinghouses on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The Set Default group and the Automatic
Report Settings group beside it had two heights, Set Default's buttons 15 px from its left and 8 px from its right, and 57 px between the
groups: Set Default as wide as its buttons and the padding (8 px on each side), the report group 8 px after it, both on the report group's
height (its content)."""


def layout(h):
    d = h.n("groupBox1")
    r = h.n("groupRecieveSettings")
    bw = 0
    for name in ("butDefaultDental", "butDefaultMedical", "butEligibility"):
        b = h.n(name)
        h.set(name, 8, None, None, None)
        bw = max(bw, b.nw)
    w = 8 + bw + 8
    h.set("groupBox1", None, None, w, r.nh)
    h.set("groupRecieveSettings", d.nx + w + 8, None, None, None)
