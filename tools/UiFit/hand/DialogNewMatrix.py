"""The report designer's DialogNewMatrix on the grid (UI-12 slice 15, layout script on the planner's tree; designer code in
DialogNewMatrix.cs), as slice 13's DialogNewChart. The planner ran the cell expression's caption into its box and left the bar 34 px
over the window's bottom; main cut Matrix Columns' caption. DataSet as a label + box row; the fields list under its caption down to the
cell expression; the columns and rows lists one under the other with one height, each with its caption (on one line, to the right
padding) over it, its arrow before it and Up, Down and Delete (64 px) after it on the right padding; Matrix Cell Expression's caption over
its box over the width, a section gap over the bar; OK and Cancel bottom right; main's 456 x 400."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    W0, H0 = 456, 400
    R = W0 - 8
    h.set(".", None, None, W0, H0)
    top = s.bar(h, right=("bOK", "bCancel"))
    lw = h.lw("label1")
    s.row(h, "label1", "cbDataSets", 8, lw, R - (8 + lw + s.ITEM), 8)
    BW = 64
    LW = (R - 8 - 8 - 32 - 8 - 8 - BW) // 2
    xl, xa = 8, 8 + LW + 8
    xr = xa + 32 + 8
    xb = R - BW
    y = 8 + h.H + 8
    s.caption(h, "label2", xl, y, LW)
    ly = s.caption(h, "label3", xr, y, R - xr)
    ey = top - s.SECTION - h.H
    cy = ey - 2 - 17
    bottom = cy - 8
    lh = (bottom - ly - 8 - 19) // 2
    h.set("lbFields", xl, ly, LW, bottom - ly)
    for lst, arrow, buts, yy in (("lbMatrixColumns", "bColumn", ("bColumnUp", "bColumnDown", "bColumnDelete"), ly),
                                 ("lbMatrixRows", "bRowSelect", ("bRowUp", "bRowDown", "bRowDelete"), bottom - lh)):
        h.set(lst, xr, yy, LW, lh)
        h.set(arrow, xa, yy, 32, 28)
        for i, b in enumerate(buts):
            h.set(b, xb, yy + 32 * i, BW, 28)
    s.caption(h, "label4", xr, bottom - lh - 19, R - xr)
    s.caption(h, "label5", 8, cy, R - 8)
    h.set("cbMatrixCell", 8, ey, R - 8, h.H)
