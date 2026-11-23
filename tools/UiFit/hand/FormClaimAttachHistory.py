"""FormClaimAttachHistory on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 14 px from the left and ended 33
px above the narrative box beside it; the right column ended 7 px short of the content edge with the character count past it, the
attachment ID box 20 px tall and Clear Attachment's text against its border: the grid on the content edge down to the narrative's bottom,
8 px before the right column; the right column to the content edge, the count right-aligned on it, the ID box at the field height with
Clear Attachment 112 px after it on the edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    x = h.n("textClaimStatus").nx
    nb = h.n("textNarrative")
    s.span(h, "gridMain", x=8, r=x - 8, b=nb.ny + nb.nh)
    s.span(h, "textClaimStatus", r=R)
    s.span(h, "textNarrative", r=R)
    c = h.n("labelCharCount")
    h.set("labelCharCount", R - c.nw, None, None, None)
    h.text_align("labelCharCount", "MiddleRight")
    y = h.n("textAttachmentID").ny
    h.set("butClear", R - 112, y, 112, h.H)
    s.span(h, "textAttachmentID", r=R - 112 - 6, b=y + h.H)
