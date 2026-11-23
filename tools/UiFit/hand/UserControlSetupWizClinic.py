"""UserControlSetupWizClinic on the grid (UI-12 slice 4, layout script on the planner's tree). The planner left the red-items note over the
end of the instructions (drawn into each other) and the right column's notes past the page's right edge (Double click a row to edit was
cut): the instructions at the content top (two lines), the red-items note on the same lines, right-aligned on the grid's right edge; the
grid 8 px under them from the content edge to 8 px before the right column, down to the content's bottom; the right column (Add, its two
notes as tall as their lines, Advanced) 140 px wide at the right content edge from the grid's top, 6 px between items."""


def layout(h):
    root = h.tree
    R = root.w - 8
    wc = 140
    xc = R - wc
    gr = xc - 8
    h.set("label8", 8, 8, 448, 30)
    h.set("label3", gr - 176, 8, 176, 30)
    h.text_align("label3", "TopRight")
    top = 8 + 30 + 8
    h.set("gridMain", 8, top, gr - 8, root.h - 8 - top)
    y = top
    h.set("butAdd", xc, y, 82, 26)
    h.set("pictureAdd", xc + 82 + 6, y + 3, None, None)
    y += 26 + 6
    h.set("label2", xc, y, wc, 32)
    y += 32 + 4
    h.set("label1", xc, y, wc, 77)
    y += 77 + 6
    h.set("butAdvanced", xc, y, 82, 26)
