"""FormLimitedStatementSelect on the grid (UI-12 slice 4, layout script on the planner's tree). The planner put the To date picker 16 px
left of the From picker (its box off the filter group's field column) with its label after the picker's start: the To picker on the From
picker's bounds x (both boxes on the column of the lists), its label right-aligned 6 px before its box."""


def layout(h):
    f = h.n("odDatePickerFrom")
    h.set("odDatePickerTo", f.nx, None)
    box = f.nx + 63
    lw = h.n("labelTo").nw
    h.set("labelTo", box - 6 - lw, None)
    h.text_align("labelTo", "MiddleRight")
