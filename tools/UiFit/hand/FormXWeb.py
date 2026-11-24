"""FormXWeb on the grid (UI-12 slice 10, layout script on the planner's tree). The right column stood at three x (246, 263 and 263 for its
boxes and captions) and the content 12 px from the left: the left column from the padding (its wide boxes to 8 px before the right column),
the right column's captions and boxes on one x ending on the right padding, the payment note across both."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    w2 = 128
    x2 = R - w2
    h.set("groupTransType", 8, None, None, None)
    for n in ("label3", "label4", "label5", "label1"):
        h.set(n, 8, None, None, None)
    for n in ("textCardNumber", "textNameOnCard"):
        h.set(n, 8, None, x2 - 8 - 8, None)
    h.set("textExpDate", 8, None, None, None)
    for n in ("label6", "label7", "labelAmount", "labelRefNumber"):
        h.set(n, x2, None, w2, None)
    for n in ("textSecurityCode", "textZipCode", "textAmount", "textRefNumber"):
        h.set(n, x2, None, w2, None)
    h.set("textPayNote", 8, None, R - 8, None)
