"""FormEhrElectronicCopy on the grid (UI-12 slice 6, layout script on the planner's tree). The content stood at main's 27 px, the note
"includes 2 files" was 192 px for its 200 px third line (cut) and the right edges ended at 345 and 405: Requested and the groups from the
padding, the note group as wide as its note, the Show group 8 px after it, the grid and Delete from the padding, the grid to the right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    h.set("butRequest", 8, None)
    h.set("label1", 8 + 103 + 6, None)
    lw = 216
    h.set("label2", None, None, lw, h.note_h("label2", lw))
    g3 = h.set("groupBox3", 8, None, lw + 16, None)
    h.set("groupBox1", 8 + lw + 16 + 8, None)
    h.set("gridMain", 8, None, R - 8, None)
    h.set("butDelete", 8, None)
