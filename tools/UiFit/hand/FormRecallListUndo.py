"""FormRecallListUndo on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). "Patients Affected:" and its count are
anchored to the bottom (Bottom|Right): the planner read LayoutManager's reset anchor in the tree and left them alone at the middle right.
Both on the OK row as on main, on its height, the count (as wide as "9999", grid-runtime-texts) 6 px before OK and the caption 6 px before
the count."""


def layout(h):
    ok = h.n("butOK")
    wc = h.lw("labelCount")
    xc = ok.nx - 6 - wc
    h.set("labelCount", xc, ok.ny, wc, ok.nh)
    w3 = h.lw("label3")
    h.set("label3", xc - 6 - w3, ok.ny, w3, ok.nh)
