"""FormEhrClinicalSummary on the grid (UI-12 slice 6, layout script on the planner's tree). The note "includes 2 files" was 192 px for its 200
px third line (cut) and the grid ended 17 px short of the groups: the note as wide as its group's content, the grid on the groups' right
edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    g = h.n("groupBox2")
    h.set("label1", None, None, g.nw - 16, h.note_h("label1", g.nw - 16))
    R = e.right_edge(h)
    h.set("gridEHRMeasureEvents", None, None, R - 8, None)
