"""FormLetterEdit on the grid (UI-12 slice 6, layout script on the planner's tree). The five-line Body of Letter label was 13 px short of its
lines and the body ended 4 px short of Save and 31 px over it: the label as tall as its lines, the body to the right edge and down to 8 px
over Save."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    l7 = h.n("label7")
    h.set("label7", None, None, None, h.note_h("label7", l7.nw))
    b = h.n("textBody")
    h.set("textBody", None, None, R - b.nx, h.n("butSave").ny - 8 - b.ny)
