"""FormRecallTypeEdit on the grid (UI-12 slice 5, layout script on the planner's tree). The code shows either the trigger rows (the list,
its caption, the disable note, Add and Remove) with the Default Interval group, or, for the Child Prophy special type, the Child Prophy
group alone (FormRecallTypeEdit.comboSpecial_SelectionChangeCommitted); main and the planner drew the group over the band's top. The
planner also put Append to Special's box on the field column with the note after its text on one line, which made the window 830 px wide
(main 646). The window at main's width; the note after the check box's text wrapped to the right content edge (two lines, the rows under
it 14 px down); the Child Prophy group on the trigger band's top with the groups' x and width; the Special Type note and the disable note
to the right content edge."""


def layout(h):
    W = 646
    R = W - 8
    dw = W - h.tree.nw
    h.set(".", None, None, W, None)
    c = h.n("checkAppendToSpecial")
    nx = c.nx + c.nw + 6
    h.set("label2", nx, c.ny, R - nx, 34)
    h.text_align("label2", "TopLeft")
    lt = h.n("listTriggers")
    dy = (c.ny + 34 + 4) - lt.ny
    for name in ("labelTriggers", "listTriggers", "labelTriggerDisable", "butAddTrigger", "butRemoveTrigger", "groupInterval", "groupBox2"):
        h.set(name, None, h.n(name).ny + dy)
    gi = h.n("groupInterval")
    h.set("groupAgeLimit", gi.nx, lt.ny, gi.nw, None)
    for name in ("labelSpecial", "labelTriggerDisable"):
        n = h.n(name)
        h.set(name, None, None, R - n.nx, None)
    for name in ("label4", "butDelete"):
        h.set(name, None, h.n(name).ny + dy)
    s = h.n("butSave")
    h.set("butSave", R - s.nw, s.ny + dy)
    h.set(".", None, None, None, h.tree.nh + dy)
