"""FormUnschedListSetup on the grid (UI-12 slice 6, layout script on the planner's tree). The note under the two fields was 9 px short of its
three lines: as tall as its lines, the group and the window growing by as much.
Fix round (ui/ui12-6-2): Days Past and Days Future end 8 px inside the group (they ended 26 px short), their captions moved with them
6 px before the boxes."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    gw = h.n("groupBox3").nw
    for box, cap in (("textDaysPast", "label14"), ("textDaysFuture", "label15")):
        b = h.n(box)
        x = gw - 8 - b.nw
        h.set(cap, h.n(cap).nx + x - b.nx, None)
        h.set(box, x, None)
    n = h.n("label1")
    nh = h.note_h("label1", n.nw) + 10
    g = h.n("groupBox3")
    gh = n.ny + nh + 8
    h.set("label1", None, None, None, nh)
    h.set("groupBox3", None, None, None, gh)
    s = h.n("butSave")
    ys = max(s.ny, g.ny + gh + 8)
    h.set("butSave", None, ys)
    if ys + s.nh + 8 > h.tree.h:
        h.set(".", None, None, None, ys + s.nh + 8)
