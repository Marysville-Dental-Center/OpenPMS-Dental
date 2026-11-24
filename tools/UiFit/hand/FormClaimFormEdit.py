"""FormClaimFormEdit on the grid (UI-12 slice 16): the claim form (panel1, its picture drawn and sized by the window's code) with the
grid's padding on the left, top and bottom; the settings column on the right (panelControls, the window's full height, its controls 8 px
from its edges, 8 px after the form): the description a caption over its box, the form size rows (labels right-aligned 6 px before one field
column, the 27 px pitch), the two check boxes a list, Font, the offset rows, the items caption with Add on its row and the list down to the
position rows (X, Y, Width, Height) on the bottom block, the internal note and Print and Save at the bottom, a section gap over them."""
import importlib.util
import os
_spec = importlib.util.spec_from_file_location("_s16", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s16.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


KEEP_BAR_GAP = True  # the bar is the column's


def layout(h):
    W, H = h.tree.w, h.tree.h
    cw = 119
    pw = cw + 2 * s.PAD
    h.set("panelControls", W - pw, 0, pw, H)
    h.set("panel1", s.PAD, s.PAD, W - pw - s.PAD, H - 2 * s.PAD)
    x = s.PAD
    lw = 48
    fx = x + lw + s.ITEM
    fw = x + cw - fx

    def row(label, field, y):
        h.text_align(label, "MiddleRight")
        h.set(label, x, y, lw, h.H)
        h.set(field, fx, y, fw, h.H)

    h.set("label1", x, s.PAD, cw, 16)
    h.set("textDescription", x, s.PAD + 18, cw, h.H)
    y = s.PAD + 18 + h.H + 4
    row("label9", "textFormWidth", y)
    row("label10", "textFormHeight", y + 27)
    y += 2 * 27
    h.set("checkIsHidden", x, y, cw, 18)
    h.set("checkPrintImages", x, y + 22, cw, 18)
    h.check_left("checkIsHidden")
    h.check_left("checkPrintImages")
    y += 22 + 18 + 4
    h.set("butFont", x, y, cw, s.BUTTON_H)
    y += s.BUTTON_H + 4
    row("label3", "textOffsetX", y)
    row("label8", "textOffsetY", y + 27)
    y += 2 * 27 + s.GAP
    h.set("butAdd", x + cw - 37, y, 37, h.H)
    h.set("label2", x, y, cw - 37 - s.ITEM, h.H)
    yList = y + h.H + 2
    yBar = H - s.PAD - s.BUTTON_H
    h.set("butPrint", x, yBar, 55, s.BUTTON_H)
    h.set("butSave", x + 55 + s.GAP, yBar, cw - 55 - s.GAP, s.BUTTON_H)
    yNote = yBar - s.SECTION - 82  # the note wraps to 5 lines in the column (82 px at 100% and 150%)
    h.set("labelInternal", x, yNote, cw, 82)
    yRows = yNote - 4 - 4 * 27 + 4
    for i, (label, field) in enumerate((("label4", "textXPos"), ("label5", "textYPos"), ("label6", "textWidth"), ("label7", "textHeight"))):
        row(label, field, yRows + i * 27)
    h.set("listItems", x, yList, cw, yRows - s.GAP - yList)
