"""FormRecallSetup (Recall Setup) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). Two touches on the
planner's layout: the Messages grid from the content edge (8, Language's x; the planner kept main's 16) to its right edge; the right column's
groups 8 px after the middle column's groups (they stood 4 px after them) to the same right edge, their fields on the groups' content edge."""


def layout(h):
    g = h.n("gridMain")
    right = g.x + g.w
    h.set("gridMain", 8, None, right - 8)
    mid = h.n("groupBox3")
    x = mid.x + mid.w + 8
    for gname in ("groupBox2", "groupBox4"):
        gb = h.n(gname)
        w = right - x
        h.set(gname, x, None, w)
        shrink = gb.w - w
        for k in gb.children:
            if k.visible and k.x + k.w >= gb.w - 8 - 1 and k.kind != "radio" and k.kind != "check":
                h.set(k.name, k.x - shrink, None)
            elif k.kind == "label" and (k.text_align or "").endswith("Right"):
                h.set(k.name, k.x - shrink, None)
    # Fix round 8: Adjust Postcard Position as tall as Recall List Default View beside it (one height for groups side by side); what stands
    # under it in the right column (Use email if, the Max # rows) moves down by as much, the window and its bottom bar too.
    g2, g3 = h.n("groupBox2"), h.n("groupBox3")
    dy = g3.nh - g2.nh
    if dy > 0:
        h.set("groupBox2", None, None, None, g3.nh)
        for name in ("groupBox4", "label7", "label4", "textMaxAutoReminders", "textMaxReminders", "butSave"):
            h.set(name, None, h.n(name).ny + dy)
        h.set(".", None, None, h.tree.nw, h.tree.nh + dy)
