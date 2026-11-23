"""FormSupplyOrders on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The supplier combo lists supplier names and
All (FormSupplyOrders.cs), and the planner sized it for its empty text (60 px): 160 px ending on the grids' right edge, its Supplier label
6 px before it and Show received 12 px before the label."""


def layout(h):
    g = h.n("gridOrders")
    R = g.nx + g.nw
    x = R - 160
    h.set("comboSupplier", x, None, 160, None)
    lab = h.n("label3")
    xl = x - 6 - lab.nw
    h.set("label3", xl, None, None, None)
    chk = h.n("checkShowReceived")
    h.set("checkShowReceived", xl - 12 - chk.nw, None, None, None)
