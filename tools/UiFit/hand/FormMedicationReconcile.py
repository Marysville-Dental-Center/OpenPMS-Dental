"""FormMedicationReconcile on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The planner moved Add Medication 29 px
left of the Medications grid it adds to and widened the window 13 px for the event note on one line (the grids then ended 26 px from the
right edge against 12 px at the left): Add Medication on the grid's x with Show Discontinued 6 px after it; the event note from its x to
the grids' right edge (two lines); the window 12 px after the grids (main's 918), the picture note under everything from the picture's x
to the grids' right edge."""
import grid_tools as g

LINE = 15


def layout(h):
    meds = h.n("gridMeds")
    add = h.n("butAdd")
    h.set("butAdd", meds.nx, None, None, None)
    h.set("checkDiscontinued", meds.nx + add.nw + 6, None, None, None)
    R = meds.nx + meds.nw
    h.set(".", None, None, R + 12, None)
    lab = h.n("label2")
    w = R - lab.nx
    h.set("label2", None, None, w, max(lab.nh, g.measured_lines(lab.text, lab.text_w, w) * LINE + 1))
    pic = h.n("pictBox")
    l3 = h.n("label3")
    w3 = R - pic.nx
    h.set("label3", pic.nx, None, w3, max(l3.nh, g.measured_lines(l3.text, l3.text_w, w3) * LINE + 1))
