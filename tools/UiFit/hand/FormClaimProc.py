"""FormClaimProc (Edit Claim Procedure) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). Two touches on the
planner's layout: the Claim Info group stands on the Carrier Allowed Amount group's left and right edges (8 px inside the estimate panel,
it stood on the panel's edge), its fields staying on the panel's field column; the Insurance Estimate row's two boxes as wide as the boxes of
the rows around them (69 and 70 px against 72), so both columns of amounts keep one left and one right edge.
Fix round 9: Blue Book Log, 6 px after the Insurance Estimate row's override box, ended 5 px past the right column's edge (the fields and
groups of the estimate panel, and Claim Info under it). It cannot end on that edge: 85 px is the narrowest that holds its text (the program's
button draws it with GDI+, 82 px at Segoe UI 9, and wraps it in less), and it cannot move left onto the override box. The right column grows
by those 5 px instead: the estimate panel, the fields and groups on its content edge (Fee Schedule, the Carrier Allowed Amount group and
its two fee schedule boxes, Estimate Note), Claim Info and the window; Save stays on the window's content edge."""


def layout(h):
    a = h.n("groupAllowed")
    p = h.n("panelEstimateInfo")
    ref = h.n("textDedEst")
    over = h.n("textDedEstOverride")
    h.set("textInsEstTotal", ref.x, None, ref.w)
    h.set("textInsEstTotalOverride", over.x, None, over.w)
    bb = h.n("butBlueBookLog")
    xb = over.x + over.w + 6
    edge = p.nw - 8
    dx = xb + bb.nw - edge
    if dx > 0:
        h.set("panelEstimateInfo", None, None, p.nw + dx, None)
        for name in ("textFeeSched", "textEstimateNote"):
            n = h.n(name)
            if n.nx + n.nw == edge:
                h.set(name, None, None, n.nw + dx, None)
        if a.nx + a.nw == edge:
            for k in a.children:
                # the group's fields that reach its right part (the two fee schedule boxes) keep their distance to its right side
                if k.visible and k.kind == "field" and k.nx + k.nw >= a.nw - 8 - 16:
                    h.set(k.name, None, None, k.nw + dx, None)
            h.set("groupAllowed", None, None, a.nw + dx, None)
        s = h.n("butSave")
        h.set("butSave", s.nx + dx, None)
        h.set(".", None, None, h.tree.nw + dx, h.tree.nh)
    h.set("butBlueBookLog", xb, None)
    g = h.n("groupClaimInfo")
    gx = p.nx + a.nx - g.x
    h.set("groupClaimInfo", g.x + gx, g.y, a.nw, g.h)
    for k in g.children:
        if k.visible and k.w > 0:
            h.set(k.name, k.x - gx, k.y)
