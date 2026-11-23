"""FormFamilyBalancer on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The window's left content edge at 8
(the planner kept main's 12 for its anchored left column; the right edge is at 8). Fix round 8: the Changed Since Date check box holds three
lines (its text has two line breaks: the last, "NOT for normal users. NOT for conversions.", is its warning), so it keeps a three-line
height with its box at the top (the planner's rule makes a check box drawn taller than one line one line tall); the Changed Since Date row
and everything under it move down by as much, the group and the window grow. The As of Date note is as wide as its widest line (its three
lines fit its 47 px, at 310 px its last line was cut)."""


def layout(h):
    h.frame_left()
    gi = h.n("groupBoxIncomeTransfers")
    ck = h.n("checkChangedSinceDate")
    # Its three lines as the program's check box draws them (GDI+, Segoe UI 9: 408 x 50) and 2 px of the line's descent.
    CH3 = 50
    dy = CH3 - ck.nh
    y0 = ck.ny + ck.nh
    for k in gi.children:
        if k is not ck and k.ny >= y0:
            h.set(k.name, None, k.ny + dy)
    h.set("checkChangedSinceDate", None, None, None, CH3)
    if ck.check_align != "TopLeft":
        ck.new_check_align = "TopLeft"
    gb = gi.ny + gi.nh
    h.set("groupBoxIncomeTransfers", None, None, None, gi.nh + dy)
    for k in h.tree.children:
        if k is not gi and k.dock in ("None", "") and k.ny >= gb:
            h.set(k.name, None, k.ny + dy)
    h.set(".", None, None, h.tree.nw, h.tree.nh + dy)
    # The As of Date note: 317 px for its widest line (TextRenderer, Segoe UI 9), to the group's content edge.
    nd = h.n("labelAsOfDateDesc")
    h.set("labelAsOfDateDesc", None, None, gi.nw - 8 - nd.nx, None)
    # The Delete All Transfers check box on the column of the date boxes and the Changed Since check box (6 px left of it), its label
    # ending 6 px before it.
    cd, ld = h.n("checkDeleteTransfers"), h.n("labelDeleteTransfers")
    xc = ck.nx
    h.set("checkDeleteTransfers", xc, None, cd.nx + cd.nw - xc, None)
    h.set("labelDeleteTransfers", None, None, xc - 6 - ld.nx, None)
    # Fix round 9: the Start Rigorous and Start FIFO rows alike: the three buttons one height (24, the dialog button height), FIFO 6 px under
    # Rigorous, both descriptions left-aligned 6 px after the buttons (the FIFO one was right-aligned, 18 px further right), centred on their
    # button, the FIFO one ending 6 px before Configure Automation; the group and what stands under it grow by what the rows need.
    br, bf, bc = h.n("butRigorous"), h.n("butFIFO"), h.n("butConfigureAuto")
    l5, l6 = h.n("label5"), h.n("label6")
    BH = 24
    yr = br.ny
    yf = yr + BH + 6
    h.set("butRigorous", None, yr, None, BH)
    h.set("butFIFO", br.nx, yf, br.nw, BH)
    h.set("butConfigureAuto", None, yf, None, BH)
    xl = br.nx + br.nw + 6
    h.set("label5", xl, yr + (BH - l5.nh) // 2, None, None)
    h.text_align("label5", "MiddleLeft")
    h.set("label6", xl, yr + BH + 6 + (BH - l5.nh) // 2, bc.nx - 6 - xl, l5.nh)
    h.text_align("label6", "MiddleLeft")
    need = yf + BH + 8 - gi.nh
    if need > 0:
        gb = gi.ny + gi.nh
        h.set("groupBoxIncomeTransfers", None, None, None, gi.nh + need)
        for k in h.tree.children:
            if k is not gi and k.dock in ("None", "") and k.ny >= gb:
                h.set(k.name, None, k.ny + need)
        h.set(".", None, None, h.tree.nw, h.tree.nh + need)
    # Recreate Splits: Delete All Transfers on the As of Date row's columns (it stood 5 px right of them): its box on the date box's x, its
    # label right-aligned 6 px before it, centred on the box's row.
    dp, la = h.n("datePickerAsOfDateRecreate"), h.n("labelAsOfDateRecreate")
    cr, lr = h.n("checkDeleteTransfersRecreate"), h.n("labelDeleteTransfersRecreate")
    h.set("checkDeleteTransfersRecreate", dp.nx, None, cr.nx + cr.nw - dp.nx, None)
    lwr = h.lw("labelDeleteTransfersRecreate")
    h.set("labelDeleteTransfersRecreate", dp.nx - 6 - lwr, cr.ny + (cr.nh - h.H) // 2, lwr, h.H)
    h.text_align("labelDeleteTransfersRecreate", "MiddleRight")
