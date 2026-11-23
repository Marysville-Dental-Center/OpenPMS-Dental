"""FormAutomationConditionEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 748
to 881 px for the warning label hidden in the designer (the code shows it in place of the text box) and left the lists at x 24 and 211:
main's width; the Field and Comparison lists from the padding 8 px apart, down to 8 px over Delete and Save, their captions over them;
the Text column 8 px after them: the text box to 6 px before its "..." button on the right padding, the warning over the same column as
tall as the lists.
Fix round (ui/ui12-8-2): Field and Comparison were centred over their lists while Text started at its box, and the captions stood 4 px
over the boxes: the three captions start where their boxes start (BottomLeft), 2 px over them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_W = 748


def layout(h):
    s.size(h, MAIN_W, None)
    R = MAIN_W - 8
    yb = s.bar(h, left=("butDelete",), right=("butSave",))
    top = h.n("listCompareField").ny
    lh = yb - 8 - top
    x = 8
    for lst, cap in (("listCompareField", "labelCompareField"), ("listComparison", "labelComparison")):
        n = h.n(lst)
        h.set(lst, x, top, None, lh)
        h.set(cap, x, None, n.nw, None)
        x += n.nw + 8
    sel = h.n("butSelect")
    h.set("butSelect", R - sel.nw, top)
    h.set("textCompareString", x, top, R - sel.nw - 6 - x, None)
    h.set("labelCompareString", x, None)
    h.set("labelWarning", x, top, R - x, lh)
    for cap in ("labelCompareField", "labelComparison", "labelCompareString"):
        h.text_align(cap, "BottomLeft")
        h.set(cap, None, 8, None, top - 2 - 8)
