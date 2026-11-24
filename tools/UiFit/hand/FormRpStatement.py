"""FormRpStatement on the grid (UI-12 slice 16): the page buttons' panel one 24 px row (the arrows and the page count centred on it,
Print as tall as the bar buttons; Print is never shown), 8 px over the bottom, centred on what is visible (the arrows and the page
count, not the panel); the preview with the grid's padding on every side, a section gap over the
panel. The window's Layout handler does the same (UiGrid.PAD, UiGrid.SECTION_GAP)."""
import importlib.util
import os
_spec = importlib.util.spec_from_file_location("_s16", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s16.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W, H = h.tree.w, h.tree.h
    p = h.n("panelZoom")
    h.set("butBack", 123, 1, 18, 22)
    h.set("labelTotPages", 123 + 18 + s.ITEM, 0, 47, 24)
    h.set("butFwd", 123 + 18 + s.ITEM + 47 + s.ITEM, 1, 18, 22)
    h.set("butPrint", p.nw - 85, 0, 85, s.BUTTON_H)
    y = H - s.PAD - s.BUTTON_H
    vis = 18 + s.ITEM + 47 + s.ITEM + 18  # the arrows and the page count: the panel's butPrint is never shown
    h.set("panelZoom", (W - vis) // 2 - 123, y, p.nw, s.BUTTON_H)  # centred on what is visible (FormRpStatement_Layout)
    s.surface(h, "printPreviewControl2", bottom=y - s.SECTION)
