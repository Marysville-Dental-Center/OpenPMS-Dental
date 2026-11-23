"""FormPodiumSetup on the grid (UI-12 slice 5, layout script on the planner's tree). The Trigger Rules boxes stood 18 px into their group
(its content edge is 8): the three boxes at the content edge, their notes 6 px after them."""


def layout(h):
    for box, lab in (("textApptSetComplete", "label4"), ("textApptTimeArrived", "label1"), ("textApptTimeDismissed", "label2")):
        b = h.n(box)
        h.set(box, 8, None)
        h.set(lab, 8 + b.nw + 6, None)
