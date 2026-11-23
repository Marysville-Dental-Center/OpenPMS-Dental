"""FormClaimAttachSnipDXC on the grid (UI-12 slice 6, layout script on the planner's tree). The warning stood at 19, the preview picture ended
76 px short of the right edge and the two Send buttons 22 and 24 px short of it, 27 px over the bottom: the warning from the padding to the
right edge, the preview picture on the right edge, Send 8 px over the bottom on the right edge with Send and Snip Another over it on that
edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    B = e.bottom_edge(h)
    h.set("labelClaimAttachWarning", 8, None, R - 8, None)
    p = h.n("pictureBoxImagePreview")
    h.set("pictureBoxImagePreview", R - p.nw, None)
    h.set("butSend", R - 75, B - 24)
    h.set("butSendAndAgain", R - 131, B - 24 - 4 - 24)
