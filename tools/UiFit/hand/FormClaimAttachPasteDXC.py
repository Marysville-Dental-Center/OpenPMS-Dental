"""FormClaimAttachPasteDXC on the grid (UI-12 slice 4, layout script on the planner's tree). The planner left the image category warning
135 px wide at the right edge, five lines in 103 px (its last line cut) and over Paste Again: the warning under the narrative box on the
right column's width (the box's x and width), as tall as its lines; Paste Again and Send on one row at the bottom of the column, ending on
the column's right edge, the row's bottom on the grid's."""


def layout(h):
    box = h.n("textNarrative")
    x, w = box.nx, box.nw
    R = x + w
    h.set("labelClaimAttachWarning", x, box.ny + box.nh + 4, w, 48)
    grid = h.n("gridMain")
    y = grid.ny + grid.nh - 24
    send = h.n("butSend")
    h.set("butSend", R - send.nw, y, send.nw, 24)
    paste = h.n("buttonPasteAgain")
    h.set("buttonPasteAgain", R - send.nw - 6 - paste.nw, y, paste.nw, 24)
