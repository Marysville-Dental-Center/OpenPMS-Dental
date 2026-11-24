"""FormClaimOverpay on the grid (UI-12 slice 6, layout script on the planner's tree). The Totals row stood 40 px over the bottom bar with the
band empty: the grid grows down so the Totals row stands 8 px over the bar.
UI-12F fix round (the review of G-UI3-ui12f): the Totals row stays 4 px under the grid it sums; the bar stands the section gap under
the Totals row, the window 4 px taller (grid_hand.bar_section_gap reads the Totals row 8 px over the bar as content and moves the bar;
the hand rows of grid-hand/6.tsv record it)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    bar = h.n("butSave").ny
    y = bar - 8 - 23
    for name in ("label1", "textOverpayTotal", "textUnderpayTotal"):
        h.set(name, None, y)
    e.bottom_to(h, "gridMain", y - 4)
