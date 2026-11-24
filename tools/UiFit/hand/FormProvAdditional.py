"""FormProvAdditional on the grid (UI-12 slice 10, layout script on the planner's tree). The grid stood 12 px from the left and the right, 14
px over the closed-merchant note: on the padding, 8 px over the note."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.fill(h, "gridProvProperties", y=8, b=h.n("labelMerchantClosedDescription").ny - 8)
