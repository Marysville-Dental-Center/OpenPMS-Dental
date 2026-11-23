"""FormLabCaseEdit on the grid (UI-12 slice 5, layout script on the planner's tree). The left column ended at five x (Patient 418, Detach
499, the Set Due Date list 500, Invoice Number and Date Time Due 508, Instructions 494): one right edge 8 px before the Tracking group
(516): the Patient and Instructions boxes to it, Detach on it with its box 6 px before, Invoice Number's box on it; the Lab and Set Due Date
lists one width 6 px apart with their captions; Date Time Due's two boxes ending on it, the weekday box on the second list's x and the label
6 px before it."""


def layout(h):
    E = h.n("groupBox1").nx - 8
    x0 = h.n("textPatient").nx
    h.set("textPatient", None, None, E - x0, None)
    for box, but in (("textAppointment", "butDetach"), ("textPlanned", "butDetachPlanned")):
        b = h.n(but)
        h.set(but, E - b.nw, None)
        h.set(box, None, None, E - b.nw - 6 - x0, None)
    t = h.n("textInvoiceNumber")
    h.set("textInvoiceNumber", E - t.nw, None)
    lab = h.n("labelInvoiceNumber")
    h.set("labelInvoiceNumber", E - t.nw - 6 - lab.nw, None)
    W = (E - x0 - 6) // 2
    x2 = E - W
    h.set("label2", x0, None, W, None)
    h.set("listLab", x0, None, W, None)
    h.set("label12", x2, None, W, None)
    h.set("listTurnaround", x2, None, W, None)
    d = h.n("textDateDue")
    h.set("textDateDue", E - d.nw, None)
    h.set("textWeekday", x2, None, E - d.nw - 6 - x2, None)
    l10 = h.n("label10")
    h.set("label10", x2 - 6 - l10.nw, None)
    h.set("textInstructions", None, None, E - x0, None)
