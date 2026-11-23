"""FormTrackNext (Planned Appointments) on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree: it left the rows
14 px under the menu and the date range off the first column), as FormUnsched's script lays out the same filters. As main arranges it, on the grid: three filter rows on the 27 px pitch from 8 px under the menu: Order by, Provider
and the clinic picker; Site and Code Range; the date range with its From box on the first field column and All Dates where main has it
in the range's bounds (190 px in: the room of its hidden week buttons). Three field columns (the second on the date range's To box, so the range's two boxes stand on the first two columns), each label
right-aligned 6 px before its column (the picker's box and the code range box on
the third, one width). Refresh in the right column on the first row; the grid from the content edge to 8 px before the column, 8 px under
the filters down to the content's bottom; Print List in the right column on the grid's bottom."""


def layout(h):
    H = h.H
    CH = h.CH
    P = H + 4
    root = h.tree
    R = root.w - 8
    y0 = h.n("menuMain").ny + h.n("menuMain").nh + 8
    y1, y2 = y0 + P, y0 + 2 * P
    DR_BOX = 65  # the date range picker's From box starts this far into it (its date picker at 2, the box at 63 in it)
    DR_TO_BOX = 300  # ... and its To box (its date picker at 237)
    F1 = 8 + DR_BOX
    h.set("label1", 8, y0, F1 - 6 - 8, H)
    h.text_align("label1", "MiddleRight")
    h.set("comboOrder", F1, y0, 133, H)
    h.set("dateRangePicker", F1 - DR_BOX, y2 - 2, None, None)  # its boxes 2 px into its bounds: on the row (review 2026-11-23)
    h.set("butAllDates", F1 - DR_BOX + 190, y2, 71, H)
    lw2 = max(h.lw("label4"), h.lw("labelSite"))
    F2 = F1 - DR_BOX + DR_TO_BOX  # the second field column on the date range's To box
    x2 = F2 - 6 - lw2
    for lab, fld, y in (("label4", "comboProv", y0), ("labelSite", "comboSite", y1)):
        h.set(lab, x2, y, lw2, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F2, y, 181, H)
    lw3 = h.lw("label6")
    F3 = F2 + 181 + 16 + lw3 + 6
    BW = 133  # the code range box keeps main's width (160 would stand 14 px from Refresh)
    cw = h.clinic_w("comboClinic")
    h.set("comboClinic", F3 - cw, y0, cw + BW, H)
    h.set("label6", F3 - 6 - lw3, y1, lw3, H)
    h.text_align("label6", "MiddleRight")
    h.set("codeRangeFilter", F3, y1, BW, None)
    RW = 86
    RX = R - RW
    h.set("butRefresh", RX, y0, RW, 24)
    yg = y2 + H + 8
    yb = root.h - 8
    h.set("gridMain", 8, yg, RX - 8 - 8, yb - yg)
    h.set("butPrint", RX, yb - 24, RW, 24)
