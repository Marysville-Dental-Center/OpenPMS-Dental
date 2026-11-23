"""FormXchargeSetup on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree). The planner left the Payment Type
combo box and the clinic picker 72 and 88 px short of the fields above them: both end on their container's field edge (the picker on the
window's, 482; Payment Type on the group's, with Username and Password)."""


def layout(h):
    p = h.n("textPath")
    c = h.n("comboClinic")
    h.set("comboClinic", None, None, p.nx + p.nw - c.nx, None)
    u = h.n("textUsername")
    t = h.n("comboPaymentType")
    h.set("comboPaymentType", None, None, u.nx + u.nw - t.nx, None)
