"""ODDateRangePicker (a shared control) on the grid (UI-12 slice 16): the two pickers at the top, 8 px apart (the sibling gap); each label
(From, To) right-aligned 6 px before its picker's box (the box 63 px into the picker) and centred on it (they ended 1 px before the box).
The week buttons' panel (shown in the vertical layout the code makes) keeps its place."""

KEEP_BAR_GAP = True


def layout(h):
    f = h.n("datePickerFrom")
    h.set("datePickerFrom", f.nx, 0)
    t = h.n("datePickerTo")
    h.set("datePickerTo", f.nx + f.nw + 8, 0)
    for label, picker in (("label1", "datePickerFrom"), ("label2", "datePickerTo")):
        p = h.n(picker)
        lw = h.n(label).nw
        h.text_align(label, "MiddleRight")
        h.set(label, p.nx + 63 - 6 - lw, 0, lw, 23)
