"""UpdateFileCopier's FormMain on the grid (UI-12 slice 14, layout script; a plain exe's window). Main: the file label 12 px in and 9 px down,
Retry (shown on an error) 12 px over the bottom. Now: the label on the padding down to a section gap over Retry, Retry on the bottom padding
in the middle (its anchor, Bottom, keeps it there). The code sizes the label to its text when it wraps (MaximumSize at its width)."""

PAD = 8


def layout(h):
    W, HH = h.tree.nw, h.tree.nh
    yb = HH - PAD - 23
    h.set("butRetry", (W - 75) // 2, yb, 75, 23)
    h.set("labelFile", PAD, PAD, W - 2 * PAD, yb - 12 - PAD)
