"""FormPopupDisplay on the grid (UI-12 slice 6, layout script on the planner's tree). The description box stood at 53, on neither column,
and the planner grew the window from main's 425 to 634 px to hold the Disable for group, which is hidden in the designer and never shown
(FormPopupDisplay.cs does not name it; on main it stands past the window's edge): main's width, the description box from the padding to
the right edge under the rows, Save on the right edge, the hidden group at main's place."""


def layout(h):
    W = 425
    h.set(".", None, None, W, None)
    R = W - 8
    h.set("groupBox1", 350, 34)
    h.set("textDescription", 8, None, R - 8, None)
    h.set("butSave", R - 75, None)
