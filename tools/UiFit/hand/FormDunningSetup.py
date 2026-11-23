"""FormDunningSetup on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree). The planner left the Filters group
at main's 12, the clinic picker's box on no column and the billing type list 34 px short of the groups beside it. The Filters group at the
content edge; in it the picker on the first row ending on the groups' right edge, the billing type caption 2 px over its list, the list
from the groups' top to the Insurance Payment Pending group's bottom; the Messages grid 8 px after the group, Add and Duplicate under it."""


def layout(h):
    H = h.H
    g1 = h.n("groupBox1")
    dx = g1.nx - 8
    top = (g1.title_bottom or 18) + 4
    g3 = h.n("groupBox3")
    g2 = h.n("groupBox2")
    c = h.n("comboClinics")
    E = g3.nx + g3.nw
    h.set("comboClinics", E - c.nw, top, None, H)
    T = top + H + 8
    h.set("groupBox3", None, T, None, None)
    h.set("groupBox2", None, T + g3.nh + 8, None, None)
    b = T + g3.nh + 8 + g2.nh
    h.set("label8", 8, T - 2 - 17, 158, 17)
    h.text_align("label8", "BottomLeft")
    h.set("listBill", 8, T, 158, b - T)
    h.set("groupBox1", 8, 8, g1.nw, b + 8)
    for name in ("gridDunning", "butAdd", "butDuplicate"):
        n = h.n(name)
        h.set(name, n.nx - dx, None, n.nw + (dx if name == "gridDunning" else 0), None)
    h.watch("groupBox1")
