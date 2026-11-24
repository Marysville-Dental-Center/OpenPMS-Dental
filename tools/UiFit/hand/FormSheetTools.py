"""FormSheetTools on the grid (UI-12 slice 9 fix round, layout script on the planner's tree): Import and Export stood at 21 and the notes
ended 12 px from the right edge. The buttons at the padding, each note 6 px after its button to the right content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.move(h, "butImport", x=8)
    s.move(h, "butExport", x=8)
    x = 8 + h.n("butImport").nw + 6
    for name in ("label2", "label1"):
        s.span(h, name, x=x, r=s.right(h))
