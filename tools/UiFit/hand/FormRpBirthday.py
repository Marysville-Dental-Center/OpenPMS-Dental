"""FormRpBirthday on the grid (UI-12 slice 8, layout script on the planner's tree). The Date Range group stood at x 21 and the Postcards
group ended 23 px short of Report's right edge: the groups from the padding 8 px apart, Postcards to the right padding with its Save Msg
and Preview buttons on its content edge and the message box to 8 px before them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    g1 = h.n("groupBox1")
    h.set("groupBox1", 8, None)
    x4 = 8 + g1.nw + 8
    gw = R - x4
    h.set("groupBox4", x4, None, gw, None)
    sb = h.n("butSave")
    xb = gw - 8 - sb.nw
    h.set("butSave", xb, None)
    h.set("butPostcards", xb, None)
    h.set("textPostcardMsg", None, None, xb - 8 - h.n("textPostcardMsg").nx, None)
