"""CodeBase's MsgBoxCopyPaste on the grid (UI-12 slice 14, layout script; its designer code is in MsgBoxCopyPaste.cs, read and edited in
InitializeComponent's body). UpdateFileCopier compiles the same file in (its window key ...@UpdateFileCopier), and the program's
MsgBoxCopyPaste is built on it (its own InitializeComponent is empty). Main: the text 12 px from the edges, Copy All 126 px in, the three
buttons 26 and 28 px tall, the bar 6 px under the text. Now: the text on the padding down to a section gap over the bar, Copy All on the
bar's left padding, Print and OK on the right padding 8 px apart, the three 26 px tall."""

PAD = 8


def layout(h):
    W, HH = h.tree.nw, h.tree.nh
    yb = HH - PAD - 26
    h.set("butCopyAll", PAD, yb, 80, 26)
    h.set("butOK", W - PAD - 75, yb, 75, 26)
    h.set("butPrint", W - PAD - 75 - 8 - 80, yb, 80, 26)
    h.set("textMain", PAD, PAD, W - 2 * PAD, yb - 12 - PAD)
