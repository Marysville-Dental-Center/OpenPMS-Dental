"""FormReactivationSetup on the grid (UI-12 slice 6, layout script on the planner's tree). The grid kept main's 16 px margin, the two option
groups stood 6 px apart with heights 78 and 81 and the combos 6 px before them: the grid from the padding to the right edge, the groups one
height on the right edge 8 px apart, the combos 8 px before them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    g = h.n("gridMain")
    h.set("gridMain", 8, None, R - 8, None)
    g1 = h.n("groupBox1")
    x1 = R - g1.nw
    h.set("groupBox1", x1, None)
    g3 = h.n("groupBox3")
    x3 = x1 - 8 - g3.nw
    h.set("groupBox3", x3, None, None, g1.nh)
    for name in ("comboStatusMailedReactivation", "comboStatusEmailedReactivation"):
        n = h.n(name)
        h.set(name, None, None, x3 - 8 - n.nx, None)
    h.set("label8", x3 + 22, None)
    h.set("textPostcardsPerSheet", x3 + 22 + 164 + 6, None)
