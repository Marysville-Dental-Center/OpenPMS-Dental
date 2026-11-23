"""FormRpPatients on the grid (UI-12 slice 5, layout script on the planner's tree). The tab control and the SQL box stood at 16 and ended
93 px short of OK's edge, and the referral lists ended 5 px above the Patient list: both from the content edge to the right content edge
(the hint box on the SELECT page to its content edge), the referral lists down to the Patient list's bottom."""


def layout(h):
    R = h.tree.nw - 8
    t = h.n("tabPatients")
    dw = (R - 8) - t.nw
    h.set("tabPatients", 8, None, R - 8, None)
    for name in ("tabData", "tabFilters"):
        p = h.n(name)
        h.set(name, None, None, p.nw + dw, None)
    b = h.n("textBox1")
    h.set("textBox1", None, None, b.nw + dw, None)
    h.set("TextSQL", 8, None, R - 8, None)
    p = h.n("listPatientSelect")
    bottom = p.ny + p.nh
    for name in ("listBoxRefType", "listReferredToSelect"):
        n = h.n(name)
        h.set(name, None, None, None, bottom - n.ny)
