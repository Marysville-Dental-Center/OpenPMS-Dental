"""FormQuickBooksOnlineAuthorization on the grid (UI-12 slice 9 fix round, layout script on the planner's tree): the address box ended at
516 against the group's and OK's right edge (525), and inside the group the two boxes left 7 px on the left and 21 on the right. The note and
the address box from the padding to the group's right edge; the group's captions and boxes from its content edge (8) to 8 px before its
right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    gr = h.n("groupSeparate")
    r = gr.nx + gr.nw
    s.span(h, "labelWaiting", x=8, r=r)
    s.span(h, "textAddress", x=8, r=r)
    for name in ("labelAuthCode", "textAuthCode", "labelRealmId", "textRealmId"):
        s.span(h, name, x=8, r=gr.nw - 8)
