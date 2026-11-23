"""FormRpAppointments on the grid (UI-12 slice 5, layout script on the planner's tree). The list columns stood at 12 and 138 and ended 46 px
above the groups beside them, the Clinics All text ran to the Date Range group, and the groups ended 8 px short of OK's edge: the groups
to the right content edge, the Providers and Clinics columns (caption, All, list) from the content edge, one width, 8 px apart, 8 px
before the groups, the lists down to the groups' bottom."""


def layout(h):
    R = h.tree.nw - 8
    g1, g2 = h.n("groupBox1"), h.n("groupBox2")
    gx = R - g1.nw
    for name in ("groupBox1", "groupBox2"):
        h.set(name, gx, None)
    w = (gx - 8 - 8 - 8) // 2
    bottom = g2.ny + g2.nh
    for i, (cap, chk, lst) in enumerate((("label1", "checkAllProvs", "listProvs"), ("labelClinics", "checkAllClinics", "listClinics"))):
        x = 8 + i * (w + 8)
        h.set(cap, x, None)
        h.set(chk, x, None)
        l = h.n(lst)
        h.set(lst, x, None, w, bottom - l.ny)
