"""The report viewer's DataSourcePassword on the grid (UI-12 slice 15, layout script; designer code in RdlViewer/DataSourcePassword.cs).
The planner wrapped the prompt to two lines and left 4 px between the box and the bar. The prompt on one line over the box (both over the
width), OK and Cancel bottom right; main's 306 x 112."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    W0 = 306
    h.set(".", None, None, W0, 112)
    s.bar(h, right=("bOK", "bCancel"))
    y = s.caption(h, "label1", 8, 8, W0 - 16)
    h.set("tbPassPhrase", 8, y, W0 - 16, h.H)
