"""FormClaimOverpay on the grid (UI-12 slice 6, layout script on the planner's tree). The Totals row stood 40 px over the bottom bar with the
band empty: the grid grows down so the Totals row stands 8 px over the bar."""
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
