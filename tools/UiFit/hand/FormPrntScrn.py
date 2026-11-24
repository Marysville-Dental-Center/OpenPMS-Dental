"""FormPrntScrn on the grid (UI-12 slice 16): the preview with the grid's padding on every side, a section gap over the bottom bar (the
window's Layout handler does the same from Print's top). The bar, bottom right: Export, Print, Zoom Out, Zoom In 8 px apart; the mouse
position boxes before them, each label 6 px before its box, a band gap (16 px) between the pairs and before the buttons. The note label
(never seen: it lies under the preview, as on main) keeps its place."""
import importlib.util
import os
_spec = importlib.util.spec_from_file_location("_s16", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s16.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    y = s.bar_right(h, ["butZoomIn", "butZoomOut", "butPrint", "butExport"], [77, 75, 75, 75])
    x = h.n("butZoomIn").nx - 16
    for label, box in (("label3", "textMouseY"), ("label2", "textMouseX")):
        x -= 56
        h.set(box, x, y, 56, h.H)
        lw = h.lw(label)
        h.text_align(label, "MiddleRight")
        h.set(label, x - s.ITEM - lw, y, lw, h.H)
        x = x - s.ITEM - lw - 16
    s.surface(h, "printPreviewControl2", bottom=y - s.SECTION)
