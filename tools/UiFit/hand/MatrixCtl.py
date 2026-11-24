"""The report designer's MatrixCtl on the grid (UI-12 slice 15, layout script on the planner's tree; designer code in MatrixCtl.cs). The
planner ran the boxes to 461, past the page's content edge (440). DataSet Name and No rows message as label + box rows to the content
edge; the Page Breaks and XML groups over the page's width under them, Page Breaks' two check boxes on one row, XML's check box over its
Cell Element Name box on the page's field column; main's 472 x 288 (a property page)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    R = s.PAGE_R
    h.set(".", None, None, 472, 288)
    lw3 = h.lw("label3")
    lw = max(h.lw("label2"), h.lw("label1"), 8 + lw3)
    fx = 8 + lw + s.ITEM  # one field column with the XML group's (its box 8 px into the group)
    s.row(h, "label2", "cbDataSet", 8, lw, R - fx, 8)
    s.row(h, "label1", "tbNoRows", 8, lw, R - fx, 35)
    gw = R - 8
    x = s.chk(h, "chkPBBefore", 8, 23)
    s.chk(h, "chkPBAfter", x + s.BAND, 23)
    h.set("groupBox1", 8, 62, gw, 23 + h.H + 8)
    y = 62 + 23 + h.H + 8 + 8
    gx = fx - 8
    s.chk(h, "chkCellContents", gx, 23)
    s.row(h, "label3", "tbCellDataElementName", gx - s.ITEM - lw3, lw3, gw - 8 - gx, 50)
    h.set("groupBox2", 8, y, gw, 50 + h.H + 8)
