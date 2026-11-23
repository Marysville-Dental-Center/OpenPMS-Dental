"""FormImagePickerDXC on the grid (UI-12 slice 6, layout script on the planner's tree). The right column's captions and boxes stood at 1021,
between the label column (994) and the field column (1080), its edges at 1215, 1219 and 1220: the field rows' labels right-aligned before
the field column, the fields to the right edge; the Claim Validation and Narrative captions and boxes from the label column's left edge to
the right edge, the count right-aligned on that edge; Send on the right edge 8 px over the bottom with Send File 6 px before it and Send and
Select Another over it; the image list and the picture from the padding down to the bottom padding, 8 px apart."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    B = e.bottom_edge(h)
    F = 1080
    x0 = 994
    for lab, fld in (("labelFileName", "textFileName"), ("labelDateTimeCreate", "textDateCreated"), ("labelImageType", "listBoxImageType")):
        h.set(fld, F, None, R - F, None)
        h.set(lab, F - 6 - h.lw(lab), None, h.lw(lab), None)
    h.set("label1", x0, None)
    h.set("textClaimStatus", x0, None, R - x0, None)
    h.set("labelNarrative", x0, None)
    h.set("labelCharCount", R - 76, None)
    ys = B - 24
    ya = ys - 4 - 24
    h.set("butSend", R - 75, ys)
    h.set("butSendFile", R - 75 - 6 - 113, ys)
    h.set("butSendAndAgain", R - 139, ya)
    n = h.n("textNarrative")
    h.set("textNarrative", x0, None, R - x0, ya - 8 - n.ny)
    h.set("elementHostImageSelector", 8, 8, 240, B - 8)
    h.set("pictureBox", 8 + 240 + 8, 8, x0 - 8 - (8 + 240 + 8), B - 8)
