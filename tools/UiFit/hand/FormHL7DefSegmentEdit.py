"""FormHL7DefSegmentEdit on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 17 px from the left, the grid and
the note box ended 8 px short of Save's edge: the grid on the content edges, the note box to the right edge. The note the code shows for a MedLab definition (grid-runtime-texts) stood between Delete and Add at 264 px for
its 472 px text on one line: from 6 px after Delete to 6 px before Add, on two lines ending on the buttons' bottom."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    s.span(h, "gridMain", x=8, r=R)
    s.span(h, "textNote", r=R)
    d = h.n("butDelete")
    a = h.n("butAdd")
    x = d.nx + d.nw + 6
    w = a.nx - 6 - x
    nh = h.note_h("labelDelete", w)
    h.set("labelDelete", x, d.ny + d.nh - nh, w, nh)
