"""FormMountDefGenerate on the grid (UI-12 slice 7, layout script on the planner's tree). The five-line item size note was 13 px too short for
its last line: as tall as its lines. Fix round (the lead's review of 2026-11-23): the Type list 185 px wide (its longest item, 'Comparison
(2 side by side)', was cut at 158 beside the scroll bar), the Photo Grid group moved right by as much."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    n = h.n("label5")
    h.set("label5", None, None, None, h.note_h("label5", n.nw))
    lst = h.n("listType")
    d = 185 - lst.nw
    h.set("listType", None, None, 185, None)
    s.move(h, "groupBoxOD1", x=h.n("groupBoxOD1").nx + d)
