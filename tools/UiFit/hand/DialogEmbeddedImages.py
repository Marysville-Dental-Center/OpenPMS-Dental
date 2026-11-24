"""The report designer's DialogEmbeddedImages on the grid (UI-12 slice 15, layout script; designer code in DialogEmbeddedImages.cs). The
planner put the name's caption over its box (overlap) and left a band of 60 px over the bar. The image list on the padding with Import,
Paste and Remove beside it; Embedded Image Name's caption over its box and the MIME type row in the right column; the image preview
under them over the width down to a section gap over the bar; OK and Cancel bottom right; main's 456 x 374. Fix round: the image list without IntegralHeight (it snapped the list 14 px short of Remove)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    W0 = 456
    R = W0 - 8
    h.set(".", None, None, W0, 374)
    top = s.bar(h, right=("bOK", "bCancel"))
    bx = 8 + 120 + 8
    for i, b in enumerate(("bImport", "bPaste", "bRemove")):
        h.set(b, bx, 8 + 27 * i, 64, h.H)
    h.set("lbImages", 8, 8, 120, 2 * 27 + h.H)
    h.integral_height_off("lbImages")
    rx = bx + 64 + s.BAND
    y = s.caption(h, "label1", rx, 8, R - rx)
    h.set("tbEIName", rx, y, R - rx, h.H)
    lw = h.lw("lDataProvider")
    y = y + h.H + 8
    s.lab(h, "lDataProvider", rx, y, lw, "MiddleLeft")
    s.lab(h, "lbMIMEType", rx + lw + s.ITEM, y, R - (rx + lw + s.ITEM), "MiddleLeft")
    y = max(8 + 2 * 27 + h.H, y + h.H) + s.SECTION
    h.set("pictureImage", 8, y, R - 8, top - s.SECTION - y)
