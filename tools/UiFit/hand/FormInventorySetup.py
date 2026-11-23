"""FormInventorySetup on the grid (UI-12 slice 6, layout script on the planner's tree). The planner put the check box on the label + field
rows' field column (244) over its note at 38: the check box at the padding, its note under its text as tall as its lines, the expiry row 8
px under it from the padding.
Fix round (ui/ui12-6-2): the expiry caption as wide as its text, left-aligned at the padding like the check box (its 230 px
right-aligned box started the text at 20), the box and its note 6 px after it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    h.check_left("checkEnabled")
    h.set("checkEnabled", 8, 8)
    nh = h.note_h("labelEnabled", R - 24)
    h.set("labelEnabled", 24, 8 + 20 + 2, R - 24, nh)
    y = 8 + 20 + 2 + nh + 8
    lw = h.lw("labelExpiryDays")
    h.set("labelExpiryDays", 8, y, lw, None)
    h.text_align("labelExpiryDays", "MiddleLeft")
    xb = 8 + lw + 6
    h.set("textExpiryDays", xb, y)
    h.set("labelExpiryNote", xb + h.n("textExpiryDays").nw + 6, y + 2)
