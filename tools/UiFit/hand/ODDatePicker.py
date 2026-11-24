"""ODDatePicker (a shared control) on the grid (UI-12 slice 16): the date box at the picker's top (y 0, as the picker's OnSizeChanged puts
it): the box is 23 px tall (the field height, from its font) in a 23 px picker, so at y 1 its bottom line was cut; the drop button 1 px
inside the box's frame. The box stays 63 px in (the label area every host aligns on); the calendar under it is UI-CAL's."""

KEEP_BAR_GAP = True


def layout(h):
    t = h.n("textDate")
    h.set("textDate", t.nx, 0)
    b = h.n("butDrop")
    h.set("butDrop", b.nx, 1)
