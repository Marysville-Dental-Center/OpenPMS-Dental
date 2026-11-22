"""FormClaimProc (Edit Claim Procedure) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). Two touches on the
planner's layout: the Claim Info group stands on the Carrier Allowed Amount group's left and right edges (8 px inside the estimate panel,
it stood on the panel's edge), its fields staying on the panel's field column; the Insurance Estimate row's two boxes as wide as the boxes of
the rows around them (69 and 70 px against 72), so both columns of amounts keep one left and one right edge."""


def layout(h):
    g = h.n("groupClaimInfo")
    a = h.n("groupAllowed")
    p = h.n("panelEstimateInfo")
    dx = p.x + a.x - g.x
    h.set("groupClaimInfo", g.x + dx, g.y, a.w, g.h)
    for k in g.children:
        if k.visible and k.w > 0:
            h.set(k.name, k.x - dx, k.y)
    ref = h.n("textDedEst")
    over = h.n("textDedEstOverride")
    h.set("textInsEstTotal", ref.x, None, ref.w)
    h.set("textInsEstTotalOverride", over.x, None, over.w)
    h.set("butBlueBookLog", over.x + over.w + 6, None)
