"""FormMaterialsUsed from main's rows on the grid (UI-12 slice 7, layout script on the planner's tree). The planner put Lot Number, OK and
Cancel on one bottom row drawn into each other (main has Lot Number on its own row over them) and the scan row 4 px under the grid: main's
448 px window; OK and Cancel 8 px apart at the bottom right, Lot Number over them on the right edge (the hidden read-only note on its row at
the left), the scan row over it (Scan, its box, Add and Remove 6 px apart, Lot and its combo box to the right edge) at the field height,
the note and the grid on the content edges, the grid 8 px over the scan row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W, H = 760, 448
    h.set(".", None, None, W, H)
    R = W - 8
    yb = H - 8 - 24
    h.set("butCancel", R - 75, yb, 75, 24)
    h.set("butOK", R - 75 - 8 - 75, yb, 75, 24)
    yl = yb - 4 - 24
    h.set("butLotNumber", R - 100, yl, 100, 24)
    h.set("labelReadOnly", 8, yl + 3, None, 18)
    ys = yl - 4 - h.H
    lw = h.lw("labelScan")
    h.set("labelScan", 8, ys, lw, h.H)
    h.text_align("labelScan", "MiddleRight")
    h.set("textScan", 8 + lw + 6, ys, None, h.H)
    for n in ("butAdd", "butRemove"):
        h.set(n, None, ys, None, h.H)
    s.row_left(h, ("textScan", "butAdd", "butRemove"), 8 + lw + 6)
    c = h.n("comboLot")
    h.set("comboLot", R - c.nw, ys, None, h.H)
    s.label_before(h, "labelLot", "comboLot")
    nh = h.note_h("labelInstructions", R - 8)
    h.set("labelInstructions", 8, 8, R - 8, nh)
    h.text_align("labelInstructions", "TopLeft")
    s.span(h, "gridMain", x=8, r=R, y=8 + nh + 4, b=ys - 8)
