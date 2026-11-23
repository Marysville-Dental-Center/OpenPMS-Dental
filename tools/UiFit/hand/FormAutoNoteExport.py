"""FormAutoNoteExport on the grid (UI-12 slice 7, layout script on the planner's tree). The tree stood 17 px from the left and 49 px before
the button column, whose 89 px buttons drew "Export Selected" against their border: the buttons 104 px on the right edge, the check box on
their x, the caption and the tree on the content edge, the tree to 8 px before the buttons and 8 px over the bottom."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    bx = s.right(h) - 104
    for n in ("butSelectAll", "butClear", "butExport"):
        h.set(n, bx, None, 104, None)
    s.move(h, "checkCollapse", x=bx)
    s.move(h, "labelExportSelect", x=8)
    s.span(h, "treeNotes", x=8, r=bx - 8, b=s.bottom(h))
