"""FormEhrEncryption on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The planner has no fixed point here (the
result box 5 px further right and the window 3 px wider on every pass): the result box under its caption (main's 389, 270 px wide) with
Transmit right-aligned on its right edge (both 659, as on main), the window 12 px after them as before the input box.
UI-12F (the final consistency pass; a window never shrinks, the lead's decision at slice 6): main's 680 px wide again: the input box
4 px wider, the middle column, the result's caption and box 4 px to the right, the result box 5 px wider (both margins 12 px)."""


def layout(h):
    cap = h.n("labelOutput")
    res = h.n("textResult")
    h.set("textResult", cap.nx, None, 270, None)
    right = cap.nx + 270
    t = h.n("butTransmit")
    h.set("butTransmit", right - t.nw, None, None, None)
    left = h.n("textInput").nx
    # UI-12F: main's 680 px wide again, the 9 px it lacked shared by the two boxes
    for name in ("butEncrypt", "label2", "butDecrypt", "labelOutput", "textResult"):
        h.set(name, h.n(name).nx + 4, None, None, None)
    h.set("textInput", None, None, h.n("textInput").nw + 4, None)
    h.set("textResult", None, None, 275, None)
    r = h.n("textResult")
    h.set("butTransmit", r.nx + r.nw - h.n("butTransmit").nw, None, None, None)
    h.set(".", None, None, 680, None)
