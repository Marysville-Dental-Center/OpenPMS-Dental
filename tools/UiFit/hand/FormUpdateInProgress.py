"""FormUpdateInProgress on the grid (UI-12 slice 9 fix round, layout script on the planner's tree): the planner moved Override to the
padding and left the warning at 15 (both stood at 15 on main), and Try Again ended 12 px from the right edge. The warning and Override at
the padding, the warning and Try Again to the right content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    r = s.right(h)
    s.span(h, "labelWarning", x=8, r=r)
    s.move(h, "butTryAgain", x=r - h.n("butTryAgain").nw)
