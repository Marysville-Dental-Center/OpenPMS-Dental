"""CodeBase's FormProgressStatus on the grid (UI-12 slice 14, layout script; a plain window with no title bar). Its constructor shows the
history box in place of the message when asked (the window 60 px wider and 120 px taller; the anchored parts follow), and Copy to
Clipboard and Close stand on the progress bar's row when it is done (the bar hidden then). Main: the message and the bar 52 to 55 px in,
the bar 19 px over the bottom, Copy 12 px in, the minimize box 1 px from the top. The planner moved the minimize box and the history box
down and left the window 8 px taller. Now, in main's 469 x 117: the minimize box in the top right corner on the padding, the history box
a row gap under it, the bar on the bottom padding over the width, the message (two lines) bottom-aligned a caption gap over the bar, Copy
and Close on the bar's row at the padding."""

PAD = 8


def layout(h):
    W, HH = h.tree.nw, h.tree.nh
    cw = W - 2 * PAD
    h.set("panelMinimize", W - PAD - 22, PAD, 22, 22)
    yb = HH - PAD - 23
    h.set("progressBar", PAD, yb, cw, 23)
    h.set("butCopyToClipboard", PAD, yb, 100, 23)
    h.set("butClose", W - PAD - 75, yb, 75, 23)
    h.text_align("labelMsg", "BottomLeft")
    h.set("labelMsg", PAD, yb - 2 - 36, cw, 36)
    yh = PAD + 22 + 4
    h.set("textHistoryMsg", PAD, yh, cw, yb - 12 - yh)
