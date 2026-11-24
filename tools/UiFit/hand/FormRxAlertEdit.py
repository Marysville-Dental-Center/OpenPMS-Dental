"""FormRxAlertEdit on the grid (UI-12 slice 10, layout script on the planner's tree). The fields outside the And-then group ended 14 px short
of its right edge (630 against 644): the group to the right padding, every field on its content edge. The default-alert note on the label
column's right edge; the window 12 px under the check box for Delete and Save (346 -> 282 high; the code does not size it)."""
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
    ln = h.n("labelName")
    s.move(h, "label1", x=ln.nx + ln.nw - h.n("label1").nw)
    c = h.n("checkIsHighSignificance")
    s.size(h, h.n(".").nw, c.ny + c.nh + 12 + h.n("butSave").nh + 8)
    s.bar(h, left=("butDelete",), right_=("butSave",))
