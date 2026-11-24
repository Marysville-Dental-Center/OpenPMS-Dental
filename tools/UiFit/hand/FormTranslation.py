"""FormTranslation on the grid (UI-12 slice 10, layout script on the planner's tree). The planner stacked Delete Selected and Delete Unused on
one place (main had them one under the other, the grid 2 px over the first): both on the bottom bar of main's 958 x 708 window, the note 6
px after Delete Unused and centred on it, the grid from the padding to 12 px over the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 958, 708)
    y = s.bar(h, left=("butDelete", "butDeleteUnused"))
    h.set("label1", h.n("butDeleteUnused").nx + h.n("butDeleteUnused").nw + 6, None, None, None)
    s.vcenter(h, "label1", "butDeleteUnused")
    s.fill(h, "gridLan", y=8, over="butDelete")
