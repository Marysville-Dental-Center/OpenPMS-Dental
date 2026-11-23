"""FormEhrLabResultEdit on the grid (UI-12 slice 6, layout script on the planner's tree). The two groups stood at 52 (main's place) while
Delete stands at the padding: the groups from the padding to the right edge with their field column kept at the window's x of Date Time's
box (158), the long fields to the groups' content edge.
Fix round (ui/ui12-6-2): Delete and Save 75 x 24 (they were 28 tall) 8 px under the Test Result group, the window's height down to 8 px
under them (a 39 px empty band; the code does not set the window's size).
Slice 8 (the lead's decision on slice 6's question: a window never shrinks): the window keeps main's 390 x 338; Delete and Save on the
bottom padding, and the height the content does not fill shared out as equal gaps between the Date Time row, the two groups and the button
row (no empty band at the bottom or over the buttons)."""

MAIN_H = 338  # main's client height (the designer before slice 6)
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
    g1 = h.n("groupBox1")
    g2 = h.n("groupBox2")
    t = h.n("textDateTimeTest")
    top = t.ny + t.nh
    yb = MAIN_H - 8 - 24
    gap = (yb - top - g1.nh - g2.nh) // 3
    y1 = top + gap
    h.set("groupBox1", None, y1)
    h.set("groupBox2", None, y1 + g1.nh + gap)
    h.set("butDelete", 8, yb, 75, 24)
    h.set("butSave", R - 75, yb, 75, 24)
    h.set(".", None, None, None, MAIN_H)
