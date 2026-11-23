"""FormEhrLabResultEdit on the grid (UI-12 slice 6, layout script on the planner's tree). The two groups stood at 52 (main's place) while
Delete stands at the padding: the groups from the padding to the right edge with their field column kept at the window's x of Date Time's
box (158), the long fields to the groups' content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    for gname in ("groupBox1", "groupBox2"):
        gn = h.n(gname)
        dx = gn.nx - 8
        h.set(gname, 8, None, R - 8, None)
        for k in gn.children:
            if k.name:
                h.set(k.name, k.nx + dx, None)
        cr = R - 8 - 8
        for name in ("textTestName", "comboAbnormalFlag"):
            if any(k.name == name for k in gn.children):
                n = h.n(name)
                h.set(name, None, None, cr - n.nx, None)
