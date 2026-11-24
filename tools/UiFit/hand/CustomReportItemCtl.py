"""The report designer's CustomReportItemCtl on the grid (UI-12 slice 15, layout script; designer code in CustomReportItemCtl.cs). A
property page (PropertyDialog's tab page, about 450 x 300): the property grid from the padding to fx, fx on the page's content edge (440,
main 447), the grid down to the page's bottom padding; main's 464 x 304."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    h.set(".", None, None, 464, 304)
    h.set("bExpr", s.PAGE_R - s.FX, 8, s.FX, h.H)
    h.set("pgProps", 8, 8, s.PAGE_R - s.FX - s.ITEM - 8, s.PAGE_B - 8)
