"""The report designer's SubreportCtl on the grid (UI-12 slice 15, layout script on the planner's tree; designer code in SubreportCtl.cs).
The planner left the parameters grid 4 px under the check box and the boxes past the page's content edge. Subreport name and No rows
message as label + box rows (the boxes to the page's content edge, "..." on it), the check box from the padding, then the parameters grid with Refresh on
the content edge, down to the page's bottom padding; main's 464 x 304."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    R = s.PAGE_R
    h.set(".", None, None, 464, 304)
    lw = max(h.lw("label1"), h.lw("label2"))
    fx = 8 + lw + s.ITEM
    s.lab(h, "label1", 8, 8, lw)
    h.set("bFile", R - 24, 8, 24, h.H)
    h.set("tbReportFile", fx, 8, R - 24 - s.ITEM - fx, h.H)
    s.row(h, "label2", "tbNoRows", 8, lw, R - fx, 35)
    s.chk(h, "chkMergeTrans", 8, 62)  # from the padding: its 379 px pass the content edge from the field column
    h.set("chkMergeTrans", None, None, s.check_w(h, "chkMergeTrans") + 8, None)  # its long text: the stock box draws it 7 px wider than GDI+ measures it
    y = 62 + h.H + 8
    h.set("bRefreshParms", R - 56, y, 56, h.H)
    h.set("dgParms", 8, y, R - 56 - 8 - 8, s.PAGE_B - y)
