"""FormRxAlertEdit on the grid (UI-12 slice 10, layout script on the planner's tree). The fields outside the And-then group ended 14 px short
of its right edge (630 against 644): the group to the right padding, every field on its content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    s.span(h, "groupBox", x=8, r=R)
    gr = R - 8
    s.span(h, "textRxName", r=h.n("groupBox").nw - 8)
    for n in ("textName", "textMessage"):
        s.span(h, n, r=gr)
