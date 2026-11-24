"""CodeBase's FormProgressExtended on the grid (UI-12 slice 14, layout script; a plain window that sizes itself to its content, AutoSize).
Its code adds one ODProgressBar per task to the table at the top of its flow panel (the gallery's state "bars" shows two); under the table
the log panel: Progress Log, the log box, the warning (shown while finishing) and Pause and Cancel. Main: the flow panel 3 px from the
window's edge, the log panel's parts 9 to 12 px in. Now: the flow panel on the padding (its children keep the flow's 3 px margins), the
log panel's parts from its edges: the caption 2 px over the box, the warning a row gap under it, the two buttons a section gap under the
warning at the panel's left and right edges; the panel as tall as that. The window is as tall as that with no bar (482 x 173; AutoSize grows it by each bar the code adds; main 469 x 262 left an empty band
of 97 px under the log: grid-main-sizes.tsv names its height code-sized)."""

PAD = 8


def layout(h):
    h.set("flowLayoutPanel1", PAD, PAD)
    pw = h.n("panel1").nw
    h.text_align("label4", "BottomLeft")
    h.set("label4", 0, 0, 96, 17)
    h.set("textMsg", 0, 19, pw, 70)
    h.set("labelWarning", 0, 19 + 70 + 4, 292, 17)
    yb = 19 + 70 + 4 + 17 + 12
    h.set("butPause", 0, yb, 75, 23)
    h.set("butCancel", pw - 75, yb, 75, 23)
    h.set("panel1", None, None, pw, yb + 23)
    # the window as tall as its content with no bar (AutoSize, GrowOnly: it grows by each bar the code adds; main's 262 left an empty
    # band under the log), as wide as the flow panel and its margins
    fl = h.n("flowLayoutPanel1")
    h.set("flowLayoutPanel1", None, None, 3 + pw + 3, 9 + (yb + 23) + 3)  # AutoSize: as big as its children and their margins
    h.set(".", None, None, PAD + 3 + pw + 3 + PAD, PAD + 9 + (yb + 23) + 3 + PAD)
