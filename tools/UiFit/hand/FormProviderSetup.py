"""FormProviderSetup (Providers) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). Three touches on the
planner's layout: the Instructors radio button in the All / Students list at the group's content edge (the planner put it on the field
column under the note); Alphabetize Providers on the Down row at the group's content edge and the buttons' height; Add and Student Bulk Edit
on the groups' right edge."""


def layout(h):
    s = h.n("radioStudents")
    h.set("radioInstructors", s.x, s.y + (s.y - h.n("radioAll").y))
    d = h.n("butDown")
    g1 = h.n("groupBox1")
    a = h.n("butAlphabetize")
    h.set("butAlphabetize", g1.w - 8 - a.w, d.y, a.w, d.h)
    right = g1.x + g1.w
    for name in ("butAdd", "butStudBulkEdit"):
        b = h.n(name)
        h.set(name, right - b.w, None)
    # Fix round 9: one content edge, 8 px inside the four groups of the right column: the fields (Classes, Last Name, First Name, ProvNum,
    # User Group) and the buttons (Create, Reassign, Move Sec, the provider pick button, Alphabetize Providers) end on it (they ended on 264,
    # 266 and 268); the Dental Schools note and the Reassign note keep their 6 px to what stands right of them.
    for gname in ("groupDentalSchools", "groupCreateUsers", "groupMovePats", "groupBox1"):
        g = h.n(gname)
        edge = g.nw - 8
        for k in g.children:
            if not k.visible or k.dock not in ("None", ""):
                continue
            r = k.nx + k.nw
            if k.kind == "field" and edge - 4 <= r < edge:
                h.set(k.name, None, None, edge - k.nx, None)
            elif k.kind == "button" and edge - 8 <= r < edge:
                h.set(k.name, edge - k.nw, None)
    lr, br = h.n("labelReassign"), h.n("butReassign")
    h.set("labelReassign", None, None, br.nx - 6 - lr.nx, None)
    n8 = h.n("label8")
    h.set("label8", None, None, h.n("groupDentalSchools").nw - 8 - n8.nx, None)
    # The Search label: right-aligned 6 px before the search box, as wide as its text (not right to left: that drew it at its left side).
    ls, ts = h.n("labelSearch"), h.n("textSearch")
    lw = h.lw("labelSearch")
    h.set("labelSearch", ts.nx - 6 - lw, None, lw, None)
    h.text_align("labelSearch", "MiddleRight")
