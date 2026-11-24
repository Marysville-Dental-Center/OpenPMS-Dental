"""The report designer's DataSetsCtl on the grid (UI-12 slice 15, layout script on the planner's tree; designer code in DataSetsCtl.cs).
Main and the planner cut "Refresh Fields" (a 52 px button on two lines). Name and Data Source on the first row, Data Source's box on the
right column ending on the page's content edge (440); SQL Select's caption on the second row with Timeout's label and box on the right
column; the SQL box under them with SQL... and Refresh Fields beside it (96 px, one line), the Fields caption and grid under it with
Delete beside it, down to the page's bottom padding; main's 448 x 304 (a property page)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    R = s.PAGE_R
    h.set(".", None, None, 448, 304)
    bw = 96
    bx = R - bw
    rfx = 280
    lw1 = h.lw("lDataSetName")
    lwd = h.lw("lDataSource")
    lwt = h.lw("label3")
    s.lab(h, "lDataSetName", 8, 8, lw1)
    h.set("tbDSName", 8 + lw1 + s.ITEM, 8, rfx - s.ITEM - lwd - s.BAND - (8 + lw1 + s.ITEM), h.H)
    s.row(h, "lDataSource", "cbDataSource", rfx - s.ITEM - lwd, lwd, R - rfx, 8)
    s.row(h, "label3", "tbTimeout", rfx - s.ITEM - lwt, lwt, 80, 35)
    h.text_align("label1", "BottomLeft")
    h.set("label1", 8, 35 + h.H - 17, 100, 17)
    y = 35 + h.H + 2
    h.set("bEditSQL", bx, y, bw, h.H)
    h.set("bRefresh", bx, y + 27, bw, h.H)
    h.set("tbSQL", 8, y, bx - 8 - 8, 80)
    y = s.caption(h, "label2", 8, y + 80 + 8, 100)
    h.set("bDeleteField", bx, y, bw, h.H)
    h.set("dgFields", 8, y, bx - 8 - 8, s.PAGE_B - y)
