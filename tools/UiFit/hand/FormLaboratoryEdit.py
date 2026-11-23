"""FormLaboratoryEdit on the grid (UI-12 slice 5, layout script on the planner's tree). The address rows ended at four x (Description and
Address 346, the City/ST/Zip row 402, Email 382, Notes 492), the turnaround grid 36 px short of Save's edge, Add and Delete at 17: one right
edge for the field rows (the Zip box's, 402: Description, Address, Email and Notes to it; Lab Slip's combo keeps its width), the grid to
the right content edge, Add and Delete at the content edge."""


def layout(h):
    z = h.n("textZip")
    E = z.nx + z.nw
    for name in ("textDescription", "textAddress", "textEmail", "textNotes"):
        n = h.n(name)
        h.set(name, None, None, E - n.nx, None)
    R = h.tree.nw - 8
    g = h.n("gridMain")
    h.set("gridMain", None, None, R - g.nx, None)
    for name in ("butAdd", "butDeleteTurnaround"):
        h.set(name, 8, None, g.nx - 6 - 8, None)
