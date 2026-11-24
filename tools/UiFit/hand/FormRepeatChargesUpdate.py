"""FormRepeatChargesUpdate on the grid (UI-12 slice 10, layout script on the planner's tree). The description (with the backdating and aging
lines the code adds) was cut at its last line: 13 lines high, from the padding to the right padding, the bar 12 px under it (the window 243
-> 264 high), Run aging centred on OK's row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 520, 264)
    h.set("labelDescription", 8, 8, s.right(h) - 8, 13 * 16 + 2)
    s.bar(h, right_=("butOK",))
    h.set("checkRunAging", 8, None, None, None)
    s.vcenter(h, "checkRunAging", "butOK")
