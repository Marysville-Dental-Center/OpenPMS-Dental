"""FormTerminalManager on the grid (UI-12 slice 6, layout script on the planner's tree). The content kept main's 18 px margin, the patient
group ended 4 px short of the right edge 27 px under the grid's top, the password group 30 px over the bottom: the note and the grid from
the padding, the patient group on the right edge at the grid's top with the grid 8 px before it, its patient and forms lines as wide as its
content (the texts the code gives them); the password group on the bottom padding with the grid down to 8 px over it.
Fix round (ui/ui12-6-2): the Forms for Kiosk list and Add or Remove Forms as wide as the captions (164, the group less 16: 8 px on both sides)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    B = e.bottom_edge(h)
    gx = R - 180
    g = h.n("gridMain")
    h.set("label1", 8, None)
    h.set("groupBoxPatient", gx, g.ny)
    for name in ("labelPatient", "labelSheets", "listSheets", "butPatForms"):
        h.set(name, None, None, 164, None)
    p = h.n("groupBoxPassword")
    h.set("groupBoxPassword", 8, B - p.nh)
    h.set("gridMain", 8, None, gx - 8 - 8, B - p.nh - 8 - g.ny)
