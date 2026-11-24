"""FormTestLatency on the grid (UI-12 slice 11, layout script on the planner's tree). The planner left the first group at main's x 12,
the three groups 157, 157 and 216 wide ending 26 px short of the right padding, the Speed note cut (its three lines in 52 px) and the Test
buttons on three heights. The three groups from the padding to the right padding, 8 px apart, one width; each note as tall as its lines at
the group's content width; the Latency and Speed boxes on one row under the taller note; the three Test buttons on the groups' bottom
padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    gw = (R - 8 - 16) // 3
    groups = ("groupBox1", "groupBox2", "groupBoxAverage")
    for i, g in enumerate(groups):
        w = gw if i < 2 else R - 8 - 2 * (gw + 8)
        h.set(g, 8 + i * (gw + 8), None, w, None)
    inner = gw - 16
    nh = max(h.note_h("label1", inner), h.note_h("label3", inner))
    for note in ("label1", "label3"):
        h.set(note, 8, 23, inner, nh)
    y = 23 + nh + 6
    for box, unit in (("textLatency", "label2"), ("textSpeed", "label4")):
        b = h.n(box)
        h.set(box, 8, y, None, h.H)
        u = h.n(unit)
        h.set(unit, 8 + b.nw + 6, y + (h.H - u.nh) // 2)
    a = h.n("groupBoxAverage")
    h.set("labelAverageExplain", 8, 23, a.nw - 16, h.note_h("labelAverageExplain", a.nw - 16))
    gh = h.n("groupBox1").nh
    for but in ("butLatency", "butSpeed", "butAverage"):
        h.set(but, 8, gh - 8 - 24, None, 24)
