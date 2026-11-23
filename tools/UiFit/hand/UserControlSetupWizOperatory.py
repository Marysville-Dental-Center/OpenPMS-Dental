"""UserControlSetupWizOperatory on the grid (UI-12 slice 6, layout script on the planner's tree). The red-items note was drawn over the end of
the instructions (main too) and the grid kept main's 18 px margin: the instructions and the note on one band at the top, the note ending on
the grid's right edge; the grid from the padding down to the bottom padding; the right column 8 px after it to the right edge (Add, its
notes, Advanced; the hidden attention picture beside Add)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    B = e.bottom_edge(h)
    cx = R - 217
    T = 8 + 34 + 8
    gr = cx - 8
    h.set("label3", gr - 175, 8, 175, 34)
    h.set("label8", 8, 8, gr - 175 - 8 - 8, 34)
    h.set("gridMain", 8, T, gr - 8, B - T)
    h.set("butAdd", cx, T)
    h.set("pictureAdd", cx + 82 + 6, T + 3)
    h.set("label2", cx, T + 26 + 4, 217, 32)
    h.set("label4", cx, T + 26 + 4 + 32 + 2, 217, 62)
    h.set("butAdvanced", cx, T + 26 + 4 + 32 + 2 + 62 + 4)
