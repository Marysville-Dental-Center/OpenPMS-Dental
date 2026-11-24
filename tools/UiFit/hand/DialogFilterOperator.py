"""The report designer's DialogFilterOperator on the grid (UI-12 slice 15, layout script on the planner's tree; its designer code is in
DialogFilterOperator.cs, read and edited in InitializeComponent's body). The planner put the label before the operator box and widened the
window to 274 for it, but left OK and Cancel where they stood (Cancel 23 px from the right edge). OK and Cancel on the bar, bottom right."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    h.set(".", None, None, 274, 201)
    s.bar(h, right=("bOK", "bCancel"))
