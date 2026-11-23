"""FormReplicationSetup on the grid (UI-12 slice 4, layout script on the planner's tree). The Set Ranges and Test notes were drawn into
each other (the Test note 2 px over the end of the Set Ranges note, which wraps onto three lines at 550 px and was cut): the two buttons
at the content edge, each note 6 px after its button to the grid's right edge (two lines each), its first line on the button's text; the
grid 3 px shorter so that the Force Sync group stays 8 px under the Test note. FormReplicationSetup_Load hides the buttons and notes and
sets the window's size without random primary keys (the group is anchored to the bottom)."""


def layout(h):
    grid = h.n("gridMain")
    R = grid.nx + grid.nw
    xl = 8 + 90 + 6
    y0 = 460
    h.set("gridMain", None, None, None, y0 - 8 - grid.ny)
    y1 = y0 + 24 + 15
    for but, lab, y in (("butSetRanges", "label1", y0), ("butTest", "label2", y1)):
        h.set(but, 8, y, 90, 24)
        h.set(lab, xl, y + 4, R - xl, 31)
        h.text_align(lab, "TopLeft")
