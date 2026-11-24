"""FormRpPrintPreview on the grid (UI-12 slice 16): the preview with the grid's padding on every side, a section gap over Previous and Next
(bottom left, 8 px apart). The window's Load does the same (UiGrid.PAD, UiGrid.SECTION_GAP from the buttons' top)."""
import importlib.util
import os
_spec = importlib.util.spec_from_file_location("_s16", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s16.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    y = h.tree.h - s.PAD - s.BUTTON_H
    h.set("butPrev", s.PAD, y, 75, s.BUTTON_H)
    h.set("butNext", s.PAD + 75 + s.GAP, y, 75, s.BUTTON_H)
    s.surface(h, "_printPreviewControl2", bottom=y - s.SECTION)
