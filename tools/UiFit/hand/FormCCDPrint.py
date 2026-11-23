"""FormCCDPrint on the grid (UI-12 slice 4, layout script; the print preview is a printout surface and keeps its place). The planner refused
the window (FormCCDPrint_Resize centres the page label with Back and Forward on the window's width whenever it is sized, so the built tree
never matches the designer) and main's bottom bar stood 2 px from the left edge with buttons 23, 26 and 27 px tall on two rows: one row 8 px
over the bottom, every button 24 px; Write Amounts To Claim at the content edge, Print Patient Copy and Print Dentist Copy 6 px apart at the
right content edge; the page label 44 px wide (two-digit pages, "99 / 99"; it was 22 px, cut from "1 / 2" on) centred on its row with Back
and Forward 6 px each side, as the code centres them; the preview down to 8 px over the bar."""


def layout(h):
    root = h.tree
    W = root.w
    y = root.h - 8 - 24
    R = W - 8
    h.set("butOverride", 8, y, None, 24)
    h.set("butPrintDentist", R - 116, y, 116, 24)
    h.set("butPrint", R - 116 - 6 - 116, y, 116, 24)
    lx = (W - 116 - 44) // 2
    h.set("labelPage", lx, y, 44, 24)
    h.text_align("labelPage", "MiddleCenter")
    h.set("butBack", lx - 6 - 75, y, 75, 24)
    h.set("butForward", lx + 44 + 6, y, 75, 24)
    h.set("printPreviewControl1", None, None, None, y - 8)
