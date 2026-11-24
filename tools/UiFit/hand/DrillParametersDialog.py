"""The report designer's DrillParametersDialog on the grid (UI-12 slice 15, layout script; designer code in DrillParametersDialog.cs).
Report name: its label before the box, "..." on the right padding; the parameters grid with Refresh beside it on the right padding, down
to a section gap over the bar; OK and Cancel bottom right; main's 464 x 248."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    W0 = 464
    R = W0 - 8
    h.set(".", None, None, W0, 248)
    top = s.bar(h, right=("bOK", "bCancel"))
    lw = h.lw("label1")
    fx = 8 + lw + s.ITEM
    s.lab(h, "label1", 8, 8, lw)
    h.set("bFile", R - 24, 8, 24, h.H)
    h.set("tbReportFile", fx, 8, R - 24 - s.ITEM - fx, h.H)
    h.set("bRefreshParms", R - 56, 35, 56, h.H)
    h.set("dgParms", 8, 35, R - 56 - 8 - 8, top - s.SECTION - 35)
