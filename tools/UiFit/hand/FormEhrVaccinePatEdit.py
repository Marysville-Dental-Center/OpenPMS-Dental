"""FormEhrVaccinePatEdit on the grid (UI-12 slice 5, layout script on the planner's tree). The right column's long rows ended at four x
(the providers' None 954, the observations grid 878, Ungroup 861, Save 966): the providers' None buttons on the right content edge with
their ... buttons and combo boxes 6 px before; the observations grid to the edge with Ungroup on it, Group 6 px before, Add on the grid's
x. The single fields keep their width (152, as the left column's)."""


def layout(h):
    R = h.tree.nw - 8
    for combo, pick, none in (("comboProvNumOrdering", "butPickProvOrdering", "butNoneProvOrdering"),
                              ("comboProvNumAdministering", "butPickProvAdministering", "butNoneProvAdministering")):
        nn = h.n(none)
        p = h.n(pick)
        c = h.n(combo)
        h.set(none, R - nn.nw, None)
        h.set(pick, R - nn.nw - 6 - p.nw, None)
        h.set(combo, None, None, R - nn.nw - 6 - p.nw - 6 - c.nx, None)
    g = h.n("gridObservations")
    h.set("gridObservations", None, None, R - g.nx, None)
    u = h.n("butUngroupObservations")
    h.set("butUngroupObservations", R - u.nw, None)
    gr = h.n("butGroupObservations")
    h.set("butGroupObservations", R - u.nw - 6 - gr.nw, None)
