"""DashboardCellCtrl on the grid (UI-12 slice 12, layout script on the planner's tree). The two button panels the cell shows over its chart
on hover (the print panel always, the edit panel in edit mode; both anchored top right) overlapped by 13 px: the print panel on the cell's
right edge, the edit panel ending where it starts, so their buttons stand in one row. The chart picture (drawn by the code) fills the cell
as before."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W = s.W(h)
    p = h.n("panelPrint")
    e = h.n("panelEditCell")
    h.set("panelPrint", W - p.nw, 0)
    h.set("panelEditCell", W - p.nw - e.nw, 0)
