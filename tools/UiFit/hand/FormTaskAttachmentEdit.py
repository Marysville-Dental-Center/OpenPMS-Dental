"""FormTaskAttachmentEdit on the grid (UI-12 slice 9, layout script on the planner's tree): the text box stood at 12 and ended 36 px short
of Save's edge: the Text caption and its box from the padding to Save's right edge.
Fix round: the box stopped 35 px over the bottom bar: down to 8 px over it, as earlier slices' grids."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    sv = h.n("butSave")
    s.move(h, "labelText", x=8)
    s.span(h, "textValue", x=8, r=sv.nx + sv.nw, b=sv.ny - 8)
