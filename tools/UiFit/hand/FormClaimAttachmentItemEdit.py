"""FormClaimAttachmentItemEdit on the grid (UI-12 slice 7, layout script on the planner's tree). The planner put the second Save's note
"(and Snip Another)" over the bottom bar, 1 px past the window's edge: one bottom row, the snip Save, its note 6 px after it and Save 8 px
after the note on the right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    sv = h.n("butSave")
    lw = h.lw("labelNewSnip")
    lx = sv.nx - 8 - lw
    h.set("labelNewSnip", lx, sv.ny, lw, sv.nh)
    h.text_align("labelNewSnip", "MiddleLeft")
    h.set("butNewSnip", lx - 6 - h.n("butNewSnip").nw, sv.ny, None, None)
