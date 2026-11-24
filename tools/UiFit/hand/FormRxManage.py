"""FormRxManage on the grid (UI-12 slice 10, layout script on the planner's tree). The grid stood 22 px from the left and the right, 41 px over
the buttons, which stood at 82 and 219: the grid on the padding and 12 px over the buttons, New Rx on the padding, Print Selected 6 px after
it, the hidden eCW error after them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    y = s.bar(h, left=("butNewRx", "butPrintSelected"))
    h.set("labelECWerror", h.n("butPrintSelected").nx + h.n("butPrintSelected").nw + 6, None, None, None)
    s.vcenter(h, "labelECWerror", "butNewRx")
    s.fill(h, "gridMain", y=8, over="butNewRx")
