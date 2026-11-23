"""FormAccountingSetup on the grid (UI-12 slice 7, layout script on the planner's tree). The planner left the notes 11 px inside the groups'
content edge, the Add and Remove buttons on another column (417) than the Change buttons (424), the payment grid 4 px short of its Add
button and 91 px of empty window under the second group: the notes on the content edge, the list, the grid and the account boxes to one
column of buttons on the groups' right edge (454), the payment grid down to the band over Save (the group grows to it)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h, "groupBox1")
    bx = R - 75
    F = h.n("listAccountsDep").nx
    s.span(h, "label2", x=8, r=R)
    h.set("label2", None, None, None, h.note_h("label2", R - 8))
    s.span(h, "label1", x=8, r=F - 6)
    s.span(h, "listAccountsDep", r=bx - 6)
    s.span(h, "textAccountInc", r=bx - 6)
    for b in ("butAdd", "butRemove", "butChange"):
        s.move(h, b, x=bx)
    h.set("butChange", None, None, 75, None)
    g2 = h.n("groupAutomaticPayment")
    grow = (h.tree.nh - 8 - 24 - 8) - (g2.ny + g2.nh)
    h.set("groupAutomaticPayment", None, None, None, g2.nh + grow)
    s.span(h, "label5", x=8, r=R)
    h.set("label5", None, None, None, h.note_h("label5", R - 8))
    s.move(h, "butAddPay", x=bx)
    gr = h.n("gridMain")
    s.span(h, "gridMain", x=8, r=R, b=gr.ny + gr.nh + grow)
    for n in ("label4", "textAccountCashInc", "butChangeCash"):
        s.move(h, n, y=h.n(n).ny + grow)
    s.span(h, "textAccountCashInc", r=bx - 6)
    s.move(h, "butChangeCash", x=bx)
    h.set("butChangeCash", None, None, 75, None)
