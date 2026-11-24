"""The report designer's TableColumnCtl on the grid (UI-12 slice 15, layout script on the planner's tree; designer code in
TableColumnCtl.cs). The planner widened the control to 555 for the Fixed Header note beside its check box. The Visibility group over
the page's width (8 to 440): its labels right-aligned in one column, the boxes after them, fx on the group's content edge; Column Width's
box under the group on the group's field column; Fixed Header on that column with its note under it, wrapped to the content edge; main's
472 x 288 (a property page)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    R = s.PAGE_R
    h.set(".", None, None, 472, 288)
    gw = R - 8
    lw = max(h.lw("label2"), h.lw("label3"))
    gx = 8 + lw + s.ITEM
    h.set("bHidden", gw - 8 - s.FX, 23, s.FX, h.H)
    s.row(h, "label2", "tbHidden", 8, lw, gw - 8 - s.FX - s.ITEM - gx, 23)
    s.row(h, "label3", "cbToggle", 8, lw, 152, 50)
    h.set("grpBoxVisibility", 8, 8, gw, 50 + h.H + 8)
    y = 8 + 50 + h.H + 8 + 8
    fx = 8 + gx
    lw1 = h.lw("label1")
    s.row(h, "label1", "tbColumnWidth", fx - s.ITEM - lw1, lw1, 100, y)
    y += 27
    s.chk(h, "chkFixedHeader", fx, y)
    s.note(h, "label4", fx, y + h.H + 4, R - fx)
