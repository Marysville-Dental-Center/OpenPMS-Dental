"""The report designer's CodeCtl on the grid (UI-12 slice 15 fix round, layout script on the planner's tree; designer code in
CodeCtl.cs). The planner widened the error list and code box (panel1) to 456 (8 to 464), past PropertyDialog's tab page of about 450 px:
the code box's vertical scroll bar was cut to 3 px. The panel over the page's width (8 to 440) and Check Syntax on its right edge; the
code box fills the panel beside the error list (docked, its designer size to match); the planner's 472 x 299."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    R = s.PAGE_R
    pw = R - 8
    h.set("panel1", 8, None, pw, None)
    bw = h.n("bCheckSyntax").nw
    h.set("bCheckSyntax", R - bw, None, bw, None)
    lx = h.n("lbErrors").nw + h.n("splitter1").nw
    h.set("tbCode", lx, None, pw - lx, None)
