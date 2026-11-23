"""FormASAP (ASAP List) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). One touch on the planner's layout:
the filter row's combo boxes at the row's top, with the Code Range filter's box (the planner centred them on the filter's 37 px, its box and
its example line under it, so they stood 7 px under their labels and the Code Range box); the Unscheduled Recalls page's date fields on
the window's field column with the Appointments page's status box and the Provider box."""


def layout(h):
    y = h.n("codeRangeFilter").y
    for name in ("comboProv", "comboClinic", "comboSite"):
        h.set(name, None, y)
    # One field column for the window and its pages: the Appointments page's status box (it cannot move left: its label is the widest), the
    # Provider box (its right edge kept, so the Clinic picker keeps its place) and the Unscheduled Recalls page's date fields.
    sc, tc = h.n("splitContainer"), h.n("tabControl")
    xw = sc.x + tc.x + h.n("tabPageAppts").x + h.n("comboAptStatus").x
    p = h.n("comboProv")
    h.set("comboProv", xw, None, p.x + p.w - xw)
    l4 = h.lw("label4")
    h.set("label4", xw - 6 - l4, y, l4, h.H)
    h.text_align("label4", "MiddleRight")
    pg = h.n("tabPageRecalls")
    # (a split container's panel stands at its 0,0 whatever the tree says)
    dx = xw - (sc.x + tc.x + pg.x + h.n("textDateStart").x)
    for name in ("textDateStart", "textDateEnd", "label1", "label2", "checkGroupFamilies"):
        h.set(name, h.n(name).x + dx, None)
