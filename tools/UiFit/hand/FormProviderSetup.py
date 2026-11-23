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
