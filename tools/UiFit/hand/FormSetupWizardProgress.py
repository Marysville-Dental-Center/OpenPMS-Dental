"""FormSetupWizardProgress on the grid (UI-12 slice 9, layout script on the planner's tree): Next and Skip ended 112 px before the window's
right edge (Back at the padding on the left): they end on the right content edge, 6 px apart."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W = h.n(".").nw
    s.row_right(h, ("butNext", "butSkip"), W - 8, gap=6)
