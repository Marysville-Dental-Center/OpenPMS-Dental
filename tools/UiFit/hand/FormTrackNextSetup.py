"""FormTrackNextSetup on the grid (UI-12 slice 10, layout script on the planner's tree). The Default Dates group stood 29 px from the left and
51 from the right: the group from the padding, as wide as its content (the Examples notes as wide as their text, the group's content edge 8
px after them), the window narrowed to the group (376 -> 300; the code does not size it), Save on the right padding.
UI-12F (the final consistency pass; a window never shrinks, the lead's decision at slice 6): main's 376 px wide again, the Default Dates
group spanning the content (its rows keep their columns)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    for n in ("label1", "label2"):
        h.set(n, None, None, h.lw(n), None)
    h.set("groupBox3", 8, None, 376 - 16, None)
    s.size(h, 376, h.n(".").nh)
    s.bar(h, right_=("butSave",))
