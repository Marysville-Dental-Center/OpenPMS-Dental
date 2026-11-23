"""FormCanadaPaymentReconciliation on the grid (UI-12 slice 7, layout script on the planner's tree). The captions, lists and boxes stood 12
px from the left and the lists ended 11 px short of Save's edge: everything from the content edge, the lists to Save's edge, the
Reconciliation Date box on that edge too with its caption over it, as wide as the caption."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    for n in ("label1", "label2", "label3", "label5", "label6", "textBillingOfficeNumber", "textTreatingOfficeNumber"):
        s.move(h, n, x=8)
    for n in ("listCarriers", "listBillingProvider", "listTreatingProvider"):
        s.span(h, n, x=8, r=R)
    for n in ("label1", "label2", "label3", "label5"):
        h.set(n, None, None, R - 8, None)
    w = h.lw("label4")
    h.set("textDateReconciliation", R - w, None, w, None)
    h.set("label4", R - w, None, w, None)
    h.set("label6", None, None, R - w - 8 - 8, None)
