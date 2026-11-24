"""The report designer's TableRowCtl on the grid (UI-12 slice 15 fix round, layout script on the planner's tree; designer code in
TableRowCtl.cs). The planner widened the Visibility group to 456 (8 to 464), past PropertyDialog's tab page of about 450 px, where its
right border was cut. As its sibling TableColumnCtl: the group over the page's width (8 to 440), its labels right-aligned in one column,
the boxes after them, fx on the group's content edge; Row Height under the group on the group's field column, its label right-aligned
before it; main's 472 x 288 (a property page)."""
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
    s.row(h, "label1", "tbRowHeight", fx - s.ITEM - lw1, lw1, 100, y)
