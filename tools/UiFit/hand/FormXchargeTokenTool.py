"""FormXchargeTokenTool on the grid (UI-12 slice 10, layout script on the planner's tree). The Total Cards label stood 28 px over its box (the
three count rows were 24, 25 and 28 px apart) and the planner grew the window 534 -> 557 high: main's size, the three count rows on the
pitch up from the bottom padding, each label 6 px before its box, Check on the bottom bar; the grid to the right padding and 12 px over the
rows."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 725, 534)
    R = s.right(h)
    s.bar(h, left=("butCheck",))
    B = s.bottom(h)
    x = h.n("textTotal").nx
    for i, (t, l) in enumerate((("textInvalid", "labelInvalid"), ("textVerified", "labelVerified"), ("textTotal", "labelTotal"))):
        h.set(t, x, B - 23 - i * 27, None, 23)
        s.label_before(h, l, t)
    h.set("labelWarning", 8, None, R - 8, None)
    s.fill(h, "gridMain", y=75, b=h.n("textTotal").ny - 12)
