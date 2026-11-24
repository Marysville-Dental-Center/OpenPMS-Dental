"""FormImagePickerPatient on the grid (UI-12 slice 16): the image tree and the picture with the grid's padding, 8 px apart; OK
bottom right in its column, 8 px from the edges, the picture 8 px before it."""
import importlib.util
import os
_spec = importlib.util.spec_from_file_location("_s16", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s16.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W, H = h.tree.w, h.tree.h
    s.bar_right(h, ["butOK"], [75])
    sel = h.n("elementHostImageSelector")
    h.set("elementHostImageSelector", s.PAD, s.PAD, sel.nw, H - 2 * s.PAD)
    x = s.PAD + sel.nw + s.GAP
    h.set("pictureBox", x, s.PAD, h.n("butOK").nx - s.GAP - x, H - 2 * s.PAD)
