"""FormHouseCalls on the grid (UI-12 slice 8, layout script on the planner's tree). The radio button and the Date Range group stood at x
42: on the padding.
Fix round (ui/ui12-8-2): the group ended at x 401 (OK ends at 478) with its buttons 37 px in from its right edge, and an 87 px empty band
stood over OK: the group from the padding to the right padding with its button column on its content edge, and the height the content does
not fill shared out as equal gaps between the radio row and the group and between the group and OK (s.spread)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    yb = s.bar(h, right=("butOK",))
    h.set("radioConfirm", 8, None)
    h.set("groupBox2", 8, None, R - 8, None)
    cr = R - 8 - 8  # the group's content edge (group coordinates)
    for name in ("but7", "butAll"):
        n = h.n(name)
        h.set(name, cr - n.nw, None)
    s.spread(h, [["radioConfirm"], ["groupBox2"]], 8, yb)
