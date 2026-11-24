"""FormSheetFieldEditPatImage on the grid (UI-12 slice 9, layout script on the planner's tree): the FieldValue group stood at 64 with its
fields on the outer column (157) and 64 px of empty group after Change: the group at the padding to the right content edge, its labels and
boxes on the outer field column (157), the boxes to 6 px before Change on the group's right content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W = h.n(".").nw
    GW = W - 16
    h.set("groupBoxOD1", 8, None, GW, None)
    F = h.n("textXPos").nx - 8
    cx = GW - 8 - h.n("butChange").nw
    s.move(h, "butChange", x=cx)
    for lab, box in (("label1", "textFieldValueDoc"), ("label2", "textFieldValueMount")):
        h.set(box, F, None, cx - 6 - F, None)
        lw = h.lw(lab)
        h.set(lab, F - 6 - lw, None, lw, None)
        h.text_align(lab, "MiddleRight")
