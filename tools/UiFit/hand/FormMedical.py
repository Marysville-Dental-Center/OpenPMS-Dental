"""FormMedical (Medical) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). On the Medical Info page the planner
left the captions of the top row's boxes where the row of Print Medical, Premedicate and the Current Meds group put them (Med Urgent 9 px
and Service Notes 17 px over their boxes, not the 2 px of the captions under them), the Service Notes box 2 px short of the Medical Summary
box's bottom and the two wide boxes ending at two x short of the page's content edge (an empty band beside them under the group). The
script puts both captions 2 px over their boxes, each starting where its box starts, the Service Notes box down to the Medical Summary box's
bottom, and the Service Notes and Medical History boxes to the page's right content edge (the group's right edge)."""


def layout(h):
    page = h.n("tabMedical")
    R = page.w - 8
    urg = h.n("textMedUrgNote")
    svc = h.n("textService")
    med = h.n("textMedical")
    comp = h.n("textMedicalComp")
    for label, box in (("label2", urg), ("label3", svc)):
        lb = h.n(label)
        h.set(label, box.x, box.y - 2 - lb.h, lb.w, lb.h)
    h.set("textService", None, None, R - svc.x, med.y + med.h - svc.y)
    h.set("textMedicalComp", None, None, R - comp.x, None)
    g = h.n("groupMedsDocumented")
    h.set("groupMedsDocumented", R - g.w, None)
