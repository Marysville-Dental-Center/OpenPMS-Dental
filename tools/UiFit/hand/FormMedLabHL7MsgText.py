"""FormMedLabHL7MsgText on the grid (UI-12 slice 7, layout script on the planner's tree). The message box stood 15 px from the left and the
file list was 6 px shorter than the five-line note beside it: the note and the box on the content edges, the list as tall as the note to
the right edge, the box 8 px under them down to 8 px over the bottom."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    n = h.n("label1")
    nh = h.note_h("label1", n.nw)
    h.set("label1", 8, 8, None, nh)
    s.span(h, "listFileNames", r=R, y=8, b=8 + nh)
    s.span(h, "textMain", x=8, r=R, y=8 + nh + 8, b=s.bottom(h))
