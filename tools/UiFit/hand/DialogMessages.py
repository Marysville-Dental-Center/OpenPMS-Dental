"""The report viewer's DialogMessages on the grid (UI-12 slice 15, layout script; designer code in RdlViewer/DialogMessages.cs). The
planner left the box 16 px from the left and 4 px over OK. The messages box over the width down to a section gap over the bar, OK bottom
right; main's 482 x 240."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    W0 = 482
    h.set(".", None, None, W0, 240)
    top = s.bar(h, right=("bOK",))
    h.set("tbMessages", 8, 8, W0 - 16, top - s.SECTION - 8)
