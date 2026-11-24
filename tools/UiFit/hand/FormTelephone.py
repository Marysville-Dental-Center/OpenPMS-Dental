"""FormTelephone on the grid (UI-12 slice 9 fix round, layout script on the planner's tree): Reformat stood at 17 and its note ended 33 px
from the right edge. Reformat at the padding, the note 6 px after it to the right content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.move(h, "butReformat", x=8)
    s.span(h, "label1", x=8 + h.n("butReformat").nw + 6, r=s.right(h))
