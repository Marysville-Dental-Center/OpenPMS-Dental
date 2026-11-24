"""The report designer's GroupingCtl on the grid (UI-12 slice 15, layout script on the planner's tree; designer code in GroupingCtl.cs).
The planner's rows place every control within the page's 440 px but widened the control to 631: a property page keeps main's 488 x 304
(PropertyDialog docks it in its tab page). Fix round: the fields (Name, the Group By grid, Label, Parent) end on one right edge and their
buttons start on one column 6 px after it; the grid's fx as tall as the other fx buttons, Delete/Up/Down a field tall, stacked 4 px apart
under it with the grid as tall as the stack; the second column of check boxes on one x, the widest first-column box and the item gap."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    h.set(".", None, None, 488, 304)
    F = 62  # the field column: the labels end 6 px before it
    R = 386  # the fields' right edge (Name, the Group By grid, Label, Parent)
    B = R + s.ITEM  # the button column: fx 6 px after the fields, Delete/Up/Down to the page's content edge
    h.set("tbName", F, None, R - F, None)
    gh = 4 * h.H + 3 * 4
    h.set("dgGroup", 8, 53, R - 8, gh)
    y = 53
    h.set("bValueExpr", B, y, s.FX, h.H)
    for b in ("bDelete", "bUp", "bDown"):
        y += h.H + 4
        h.set(b, B, y, s.PAGE_R - B, h.H)
    y = 53 + gh + 8
    for lb, cb, bt in (("label2", "cbLabelExpr", "bLabelExpr"), ("lParent", "cbParentExpr", "bParentExpr")):
        h.set(lb, None, y, None, h.H)
        h.set(cb, F, y, R - F, None)
        h.set(bt, B, y, s.FX, h.H)
        y += 27
    x2 = F + max(h.n(c).nw for c in ("chkPBS", "chkGrpHeader", "chkGrpFooter")) + s.ITEM
    for left, right in (("chkPBS", "chkPBE"), ("chkGrpHeader", "chkRepeatHeader"), ("chkGrpFooter", "chkRepeatFooter")):
        h.set(left, F, y, None, None)
        h.set(right, x2, y, None, None)
        y += 22
