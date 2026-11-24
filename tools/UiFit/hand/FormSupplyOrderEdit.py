"""FormSupplyOrderEdit on the grid (UI-12 slice 10, layout script on the planner's tree). The two date boxes at two widths (Date Placed 92,
Date Received 100): Date Placed as wide as Date Received (100, as FormSupplyOrderItemEdit's), its note 6 px after it; the window 8 px wider
so the note keeps its one line (715 -> 723; the code does not size it), Delete and Save on the bottom bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    h.set("textDatePlaced", None, None, h.n("textDateReceived").nw, None)
    t = h.n("textDatePlaced")
    s.move(h, "label3", x=t.nx + t.nw + 6)
    l3 = h.n("label3")
    s.size(h, l3.nx + l3.nw + 8, h.n(".").nh)
    s.bar(h, left=("butDelete",), right_=("butSave",))
