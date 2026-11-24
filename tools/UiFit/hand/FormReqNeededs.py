"""FormReqNeededs on the grid (UI-12 slice 10, layout script on the planner's tree). The grid stood 16 px from the left and 16 px before the
groups, 19 px over the bar; Copy was 23 px high beside Add's 26 (the two groups 3 px apart in height): on the padding, 8 px before the
groups (on the right padding), 12 px over the bar; Copy as high as Add."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    h.set("butCopy", None, None, None, 26)
    h.set("groupBox1", None, None, None, h.n("groupBox3").nh)
    for gname in ("groupBox3", "groupBox1"):
        s.move(h, gname, x=R - h.n(gname).nw)
    s.fill(h, "gridMain", y=8, r=h.n("groupBox3").nx - 8, over="butSave")
