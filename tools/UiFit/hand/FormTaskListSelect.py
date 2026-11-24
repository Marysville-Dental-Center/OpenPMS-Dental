"""FormTaskListSelect on the grid (UI-12 slice 10, layout script on the planner's tree). The grid ended 104 px short of the right padding, the
hidden Send-copies check box (shown in the multiple-lists mode) 4 px under it: the grid to the right padding, the check box on OK's row, the
grid 12 px over the row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    y = s.bar(h, right_=("butOK",))
    h.set("checkMulti", 8, None, None, None)
    s.vcenter(h, "checkMulti", "butOK")
    s.fill(h, "gridMain", y=h.n("gridMain").ny, over="butOK")
