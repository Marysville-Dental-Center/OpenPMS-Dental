"""FormPopupsForFam on the grid (UI-12 slice 10, layout script on the planner's tree). The grid stood 24 px from the left, 17 from the right
and 26 px over Add; Show Deleted (anchored bottom right) stood in the middle of the bar: the grid on the padding and 12 px over Add, Show
Deleted on the right padding centred on Add's row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    c = h.n("checkDeleted")
    w = c.text_w + 20
    h.set("checkDeleted", R - w, None, w, None)
    s.vcenter(h, "checkDeleted", "butAdd")
    s.fill(h, "gridMain", y=8, over="butAdd")
