"""The report designer's GroupingCtl on the grid (UI-12 slice 15, layout script on the planner's tree; designer code in GroupingCtl.cs).
The planner's rows place every control within the page's 440 px but widened the control to 631: a property page keeps main's 488 x 304
(PropertyDialog docks it in its tab page)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    h.set(".", None, None, 488, 304)
