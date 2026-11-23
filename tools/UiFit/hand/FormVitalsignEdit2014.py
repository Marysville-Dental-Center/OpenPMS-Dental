"""FormVitalsignEdit2014 on the grid (UI-12 slice 5, layout script on the planner's tree). In the Exclusion group the planner kept the
medical-reason check box on one line (388 px), so its Reason Code and Description stood on a second field column (498) beside the
pregnancy column (381); the pregnancy notice (hidden; shown by the code with a pregnancy code) stood over the check box's third and fourth
lines (main too). The notice under the pregnancy check box's four lines, the medical-reason check box wrapped to the same width under it,
Reason Code and its Description on the pregnancy column; the group grows by what that adds and everything under it moves down;
the window at main's width again.

Fix round 2: Change Default (89 px for its 89 px text) left its text 2-3 px from the border: its text and 8 px on each side, its right
edge kept on the Description boxes' edge, the Pregnancy Code box narrower by as much (6 px before it)."""


def layout(h):
    H = h.H
    cp = h.n("checkPregnant")
    pn = h.n("labelPregNotice")
    yn = cp.ny + cp.nh + 2
    h.set("labelPregNotice", None, yn)
    cn = h.n("checkNotPerf")
    y2 = yn + pn.nh + 8
    h.set("checkNotPerf", cp.nx, y2, cp.nw, 34)
    fx = h.n("textPregCode").nx
    lx = h.n("label6").nx + h.n("label6").nw
    for lab, fld, yy in (("labelNotPerf", "textReasonCode", y2), ("label4", "textReasonDescript", y2 + H + 4)):
        lw = h.lw(lab)
        h.set(lab, lx - lw, yy, lw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, fx, yy, None, None)
    d = h.n("textReasonDescript")
    g = h.n("groupExclusion")
    gh = d.ny + d.nh + 8
    dy = gh - g.nh
    h.set("groupExclusion", None, None, None, gh)
    for name in ("groupInterventions", "labelNotes", "textNotes", "labelBMIPercentile", "textBMIPercentile", "labelBMIPercentileCode",
                 "textBMIPercentileCode", "butDelete", "butSave"):
        h.set(name, None, h.n(name).ny + dy)
    # With the reason check box wrapped nothing needs the 717 px the planner gave the window: main's 602 again, the two groups to its
    # content edge and Save on it.
    W = 602
    for name in ("groupExclusion", "groupInterventions"):
        h.set(name, None, None, W - 8 - 8, None)
    s = h.n("butSave")
    h.set("butSave", W - 8 - s.nw, None)
    h.set(".", None, None, W, h.tree.nh + dy)
    b = h.n("butChangeDefault")
    r = b.nx + b.nw
    bw = b.text_w + 16
    h.set("butChangeDefault", r - bw, None, bw, None)
    pc = h.n("textPregCode")
    h.set("textPregCode", None, None, r - bw - 6 - pc.nx, None)
