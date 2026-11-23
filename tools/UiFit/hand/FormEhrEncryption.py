"""FormEhrEncryption on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The planner has no fixed point here (the
result box 5 px further right and the window 3 px wider on every pass): the result box under its caption (main's 389, 270 px wide) with
Transmit right-aligned on its right edge (both 659, as on main), the window 12 px after them as before the input box."""


def layout(h):
    cap = h.n("labelOutput")
    res = h.n("textResult")
    h.set("textResult", cap.nx, None, 270, None)
    right = cap.nx + 270
    t = h.n("butTransmit")
    h.set("butTransmit", right - t.nw, None, None, None)
    left = h.n("textInput").nx
    h.set(".", None, None, right + left, None)
