"""FormEhrPatientSmoking on the grid (UI-12 slice 3, mode hand: the planner put the filter choices before the field column, at two x in
the two groups). Main's arrangement on the grid's numbers: Current Smoking Status with its box and the MU note after it; the screening
group at the padding to the right edge down to 8 px over Save, its two groups one height filling it; in each the fields on one field column
(the window's), the filter row's choices on the column 16 px apart, Patient Declined on the column, the add button on the column, the
history grid 16 px after the fields to the content edge, the group's content height. The two filter rows share their x stops (each choice
16 px after the wider of the two choices before it), so the choices of the two groups stand on common columns."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    CH = h.CH
    by = HH - PAD - 24
    lw0 = h.lw("label2")
    F = PAD + lw0 + 6
    h.set("label2", PAD, PAD, lw0, H)
    h.text_align("label2", "MiddleRight")
    h.set("comboSmokeStatus", F, PAD, h.n("comboSmokeStatus").nw, H)
    nx = F + h.n("comboSmokeStatus").nw + 6
    h.set("label1", nx, PAD, h.lw("label1"), H)
    h.text_align("label1", "MiddleLeft")
    gy = PAD + H + GAP
    gh = by - GAP - gy
    gw = R - PAD
    h.set("groupTobaccoUse", PAD, gy, gw, gh)
    go = h.n("groupTobaccoUse")
    top = (go.title_bottom or 18) + 4
    iw = gw - 2 * PAD
    ih = (gh - top - GAP - PAD) // 2
    Fi = F - 2 * PAD  # the window's column in the inner groups
    ilw = Fi - 6 - PAD
    grids = []
    gx = 0
    # The filter rows' shared x stops: the n-th choice of both rows on one x, 16 px after the wider n-1-th choice.
    radio_rows = (("radioAllStatuses", "radioUserStatuses", "radioNonUserStatuses", "radioRecentStatuses"),
                  ("radioAllInterventions", "radioMedInterventions", "radioCounselInterventions", "radioRecentInterventions"))
    stops = [Fi]
    for i in range(1, len(radio_rows[0])):
        stops.append(stops[-1] + max(h.cw(rr[i - 1]) for rr in radio_rows) + 16)
    for k, (grp, rows, grid) in enumerate((
            ("groupAssessment", (("f", "label6", "textDateAssessed", 140), ("f", "label5", "comboAssessmentType", 260),
                                 ("r", "label8", ("radioAllStatuses", "radioUserStatuses", "radioNonUserStatuses", "radioRecentStatuses")),
                                 ("f", "labelTobaccoStatus", "comboTobaccoStatus", 260), ("b", "butAddAssessment")), "gridAssessments"),
            ("groupIntervention", (("f", "label4", "textDateIntervention", 140),
                                   ("r", "label7", ("radioAllInterventions", "radioMedInterventions", "radioCounselInterventions",
                                                    "radioRecentInterventions")),
                                   ("f", "label3", "comboInterventionCode", 260), ("c", "checkPatientDeclined"), ("b", "butAddIntervention")),
             "gridInterventions"))):
        g = h.n(grp)
        t = (g.title_bottom or 18) + 4
        y = t
        right = 0
        for r in rows:
            if r[0] == "f":
                h.set(r[1], PAD, y, ilw, H)
                h.text_align(r[1], "MiddleRight")
                h.set(r[2], Fi, y, r[3], H)
                right = max(right, Fi + r[3])
                y += H + 4
            elif r[0] == "r":
                h.set(r[1], PAD, y, ilw, H)
                h.text_align(r[1], "MiddleRight")
                for i, rad in enumerate(r[2]):
                    h.check_left(rad)
                    h.set(rad, stops[i], y + (H - CH) // 2, h.cw(rad), CH)
                    right = max(right, stops[i] + h.cw(rad))
                y += H + 4
            elif r[0] == "c":
                h.check_left(r[1])
                h.set(r[1], Fi, y, h.cw(r[1]), CH)
                y += CH + 4
            else:
                h.set(r[1], Fi, y, 104, H)
                y += H + 4
        grids.append((grid, t))
        gx = max(gx, right + 16)
        h.set(grp, PAD, top + k * (ih + GAP), iw, ih)
    # The grids keep their designer MinimumSize (559 px): the window grows right when the fields need more room.
    need = gx + max(h.n(g_).nw for g_, t in grids) + PAD
    if need > iw:
        iw = need
        gw = iw + 2 * PAD
        W = gw + 2 * PAD
        R = W - PAD
        h.set("groupTobaccoUse", PAD, gy, gw, gh)
        for k, (g_, t) in enumerate(grids):
            h.set(("groupAssessment", "groupIntervention")[k], PAD, top + k * (ih + GAP), iw, ih)
        h.set(".", None, None, W, HH)
    for grid, t in grids:  # both grids on one column
        h.set(grid, gx, t, iw - PAD - gx, ih - t - PAD)
    h.set("butSave", R - 76, by, 76, 24)
    h.watch(".", "groupTobaccoUse", "groupAssessment", "groupIntervention")
