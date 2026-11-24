"""FormSheetFieldSpecial on the grid (UI-12 slice 9, layout script on the planner's tree): the special-field note (the code sets the tooth
chart's longer text, grid-runtime-texts/9.tsv) had room for two lines in 207 px: the note from the field rows' label column to Save's right
edge, as tall as its longest text; the X Pos, Y Pos, Width and Height rows and the Growth Behavior row the code shows on the pitch 8 px
under it; the list and its caption at the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    sv = h.n("butSave")
    R = sv.nx + sv.nw
    X = h.n("labelGrowth").nx
    nw = R - X
    nh = h.note_h("labelSpecialInfo", nw)
    h.set("labelSpecialInfo", X, 8, nw, nh)
    y = 8 + nh + 8
    for lab, box in (("label5", "textXPos"), ("label6", "textYPos"), ("label7", "textWidth"), ("label8", "textHeight"), ("labelGrowth", "comboGrowthBehavior")):
        s.move(h, box, y=y)
        s.move(h, lab, y=y)
        y += h.H + 4
    s.move(h, "label2", x=8)
    s.move(h, "listBoxAvailable", x=8)
