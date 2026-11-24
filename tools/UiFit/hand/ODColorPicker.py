"""ODColorPicker (a shared control) on the grid (UI-12 slice 16): the colour button and None one height (21, the control's), 6 px apart
(the item gap; they stood 3 px apart at two heights), within the control's 74 px."""

KEEP_BAR_GAP = True


def layout(h):
    h.set("butColor", 0, 0, 28, 21)
    h.set("butNone", 34, 0, 40, 21)
