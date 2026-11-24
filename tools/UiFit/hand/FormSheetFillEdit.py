"""FormSheetFillEdit on the grid (UI-12 slice 16): the sheet (panelScroll, drawn by the window's code inside) with the grid's padding
on the left, top and right and a section gap over the controls under it. Under it, from the left: the date and the description, each a
caption 2 px over its box, and Show in Terminal (its label 6 px before its box) on the bottom padding; the note, a caption over its box down
to the bottom padding; then the button columns 8 px apart (Erase, Unlock, Print or Email; PDF, Print; To Kiosk, Change Patient, Restore,
Email; Add Field, Reduce Font Size and its help button 6 px after it), four rows of buttons 4 px apart ending on the bottom padding, and
Delete, Save, OK bottom right in the same rows."""
import importlib.util
import os
_spec = importlib.util.spec_from_file_location("_s16", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s16.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


KEEP_BAR_GAP = True  # the controls under the sheet are rows of one block; the sheet stands a section gap over the block


def layout(h):
    W, H = h.tree.w, h.tree.h
    B = s.BUTTON_H
    rows = [H - s.PAD - B - i * (B + 4) for i in (3, 2, 1, 0)]  # the four button rows, top to bottom
    yTop = rows[0] - 5  # the block's top: the captions
    # the left column
    h.set("labelDateTime", s.PAD, yTop, 146, 16)
    h.set("textDateTime", s.PAD, yTop + 18, 146, h.H)
    yd = yTop + 18 + h.H + 4
    h.set("labelDescription", s.PAD, yd, 146, 16)
    h.set("textDescription", s.PAD, yd + 18, 146, h.H)
    ys = H - s.PAD - h.H
    h.set("textShowInTerminal", s.PAD + 146 - 29, ys, 29, h.H)
    lw = h.lw("labelShowInTerminal")
    h.text_align("labelShowInTerminal", "MiddleRight")
    h.set("labelShowInTerminal", s.PAD + 146 - 29 - s.ITEM - lw, ys, lw, h.H)
    # the note
    xn = s.PAD + 146 + s.GAP
    h.set("labelNote", xn, yTop, 146, 16)
    h.set("textNote", xn, yTop + 18, 146, H - s.PAD - (yTop + 18))
    # the button columns
    x1 = xn + 146 + s.GAP + 4
    cols = [x1, x1 + 81 + s.GAP, x1 + 2 * (81 + s.GAP), x1 + 3 * (81 + s.GAP)]
    h.set("checkErase", cols[0], rows[1] + (B - 18) // 2, 81, 18)
    h.check_left("checkErase")
    h.set("butUnlock", cols[0], rows[2], 81, B)
    h.set("butPrintOrEmail", cols[0], rows[3], 81, B)
    h.set("butPDF", cols[1], rows[2], 81, B)
    h.set("butPrint", cols[1], rows[3], 81, B)
    h.set("butToKiosk", cols[2], rows[0], 81, B)
    h.set("butChangePat", cols[2], rows[1], 81, B)
    h.set("butRestore", cols[2], rows[2], 81, B)
    h.set("butEmail", cols[2], rows[3], 81, B)
    w3 = 108  # the last column one width: Reduce Font Size with the button padding at 150%
    h.set("butAddField", cols[3], rows[1], w3, B)
    h.set("butReduceFontSize", cols[3], rows[2], w3, B)
    h.set("butFontAbout", cols[3] + w3 + s.ITEM, rows[2], 24, B)
    xr = W - s.PAD - 81
    h.set("butDelete", xr, rows[1], 81, B)
    h.set("butSave", xr, rows[2], 81, B)
    h.set("butOK", xr, rows[3], 81, B)
    h.set("panelScroll", s.PAD, s.PAD, W - 2 * s.PAD, yTop - s.SECTION - s.PAD)
