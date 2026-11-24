"""CodeBase's ODProgressBar on the grid (UI-12 slice 14, layout script): one bar of the progress window (FormProgressExtended adds one per
task to its table), its name before it, its percent after it, its title over it; LayoutHelper hides any of the labels and stretches the
bar into a hidden one's room (ITEM_GAP, its padding of 6 px) or shortens the control by the hidden title. Main: the name 3 px in and 3 px
lower than the bar (off its centre), the percent 4 px lower, the title 6 px in at the top. Now, in main's 470 x 51: the name, the bar and
the percent on one row (the name right-aligned an item gap before the bar, the percent an item gap after it, both centred on it), the
title over the bar's width a caption gap over it, the two rows centred in the control's height."""


def layout(h):
    W, HH = h.tree.nw, h.tree.nh
    yt = (HH - (17 + 2 + 23)) // 2
    yb = yt + 17 + 2
    h.text_align("labelLeftText", "MiddleRight")
    h.set("labelLeftText", 0, yb, 98, 23)
    h.set("progressBar", 104, yb, 286, 23)
    h.text_align("labelPercentComplete", "MiddleLeft")
    h.set("labelPercentComplete", 396, yb, W - 396, 23)
    h.text_align("labelTopText", "BottomCenter")
    h.set("labelTopText", 104, yt, 286, 17)
