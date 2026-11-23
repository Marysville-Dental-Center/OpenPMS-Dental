"""FormEHR on the grid (UI-12 slice 5, layout script on the planner's tree). "Provider for this patient:" was cut by the value label that
starts 13 px into it, and the two rows' labels and values stood at two x (12 and 8; 135 and 142): one label column as wide as the longer
label, the values 6 px after it. The side buttons were 84 px with their texts touching the edges (Not Performed, Clinical Summ.) and stood
at the groups' left edge while the buttons in the groups stood 8 px in: the free buttons as wide as the groups (the column to the right
content edge), the buttons in the groups to their content edges."""


def layout(h):
    lw = max(h.lw("label1"), h.lw("label2"))
    for lab, val, y in (("label1", "labelProvPat", 8), ("label2", "labelProvUser", 31)):
        h.set(lab, 8, y, lw, 18)
        h.text_align(lab, "MiddleLeft")
        h.set(val, 8 + lw + 6, y)
    R = h.tree.nw - 8
    x = h.n("groupBox4").nx
    w = R - x
    for name in ("groupBox4", "groupBox5", "groupBox1"):
        h.set(name, x, None, w, None)
    for name in ("butMeasures", "but2014CQM", "butHash", "butEncryption", "but2011Labs"):
        h.set(name, 8, None, w - 16, None)
    for name in ("butVaccines", "butAmendments", "butEhrNotPerformed", "butEncounters", "butInterventions", "butCarePlans",
                 "butClinicalSummary", "butPatList", "butMeasureEvent"):
        h.set(name, x, None, w, None)
