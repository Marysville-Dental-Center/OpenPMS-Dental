"""FormInsPlansMergeWarning on the grid (UI-12 slice 7, layout script on the planner's tree). The grid ended 18 px from the right edge (8 from
the left), the note 27 px: both to the right edge (the note's text is set in Load: grid-runtime-texts). Fix round (the lead's review of
2026-11-23): Combine 8 px before Cancel, the dialog buttons' gap (it stood 13 px before it)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "gridMain", r=s.right(h))
    s.span(h, "labelWarning", r=s.right(h))
    c = h.n("butCancel")
    s.move(h, "butCombine", x=c.nx - 8 - h.n("butCombine").nw)
