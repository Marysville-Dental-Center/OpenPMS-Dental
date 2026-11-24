"""FormPatientStatusTool on the grid (UI-12 slice 4, layout script on the planner's tree). The Since label stood 16 px tall at the top of
its row (its text 4 px over the date box's) and the bottom bar's Change Patient Status To combo 36 px off the criteria's date column
(GridRulesTests' column check): the Since label on the row, right-aligned 6 px before the date box; the bottom combo on the clinic picker's
box column (its label right-aligned 6 px before it), Select All, Deselect All and Run as the planner placed them.
Fix round: Run and Create List right-aligned on the grid's right edge (main's; the planner left them 7 px short), the window 8 px
under the bottom row (the planner moved the row up 21 px and left the window's height, 36 px under it).
UI-12F (the final consistency pass): main's 639 px high again (a window never shrinks): the bottom row on the bottom padding and
the grid down to a section gap (12 px) over it (it ended 4 px over the row).
UI-12F fix round (the review of G-UI3-ui12f): hand rows after the script (rule "by hand (UI-12F fix round)") put the column on the grid's
8 px padding on both sides (it stood 19 px from the left, 12 from the right): the Criteria group and the grid 15 px wider."""


def layout(h):
    p = h.n("odDatePickerSince")
    lab = h.n("label1")
    lw = h.lw("label1")
    h.set("label1", p.nx + 63 - 6 - lw, p.ny, lw, h.H)
    h.text_align("label1", "MiddleRight")
    g = h.n("groupCriteria")
    c = h.n("comboClinic")
    x = g.nx + c.nx + h.clinic_w("comboClinic")
    h.set("comboChangePatientStatusTo", x, None)
    lw2 = h.n("labelChangeTo").nw
    h.set("labelChangeTo", x - 6 - lw2, None)
    grid = h.n("gridMain")
    R = grid.nx + grid.nw
    for name in ("butRun", "butCreateList"):
        h.set(name, R - h.n(name).nw, None)
    run = h.n("butRun")
    yb = 639 - 8 - run.nh  # UI-12F: main's height, the bottom row on the bottom padding
    for name in ("butSelectAll", "butDeselectAll", "labelChangeTo", "comboChangePatientStatusTo", "butRun"):
        h.set(name, None, yb, None, None)
    h.set("gridMain", None, None, None, yb - 12 - grid.ny)
    h.set(".", None, None, None, 639)
