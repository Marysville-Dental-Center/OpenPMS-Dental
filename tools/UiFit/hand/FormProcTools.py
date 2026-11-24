"""FormProcTools on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the window 698 -> 1279 wide to put every
check box's text on one line (a 160 px empty band under them): main's 698 px, the check boxes wrapped to their measured lines (main wrapped
them too), one list from the padding (4 px apart), the two rules 6 px under D Codes and 14 px apart as on main."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 698, 460)
    R = s.right(h)
    h.set("label5", 8, 8, R - 8, None)
    y = 8 + h.n("label5").nh + 4
    def chk(name, y):
        hh = max(20, h.note_h(name, R - 8 - 16))
        h.check_left(name)
        h.set(name, 8, y, R - 8, hh)
        return y + hh + 4
    for n in ("checkTcodes", "checkNcodes", "checkCodeImport"):
        y = chk(n, y)
    h.set("labelLineOne", 8, y + 2, R - 8, 2)
    h.set("labelLineTwo", 8, y + 16, R - 8, 2)
    y += 16 + 2 + 6
    for n in ("checkAutocodes", "checkCodeGroups", "checkProcButtons", "checkApptProcsQuickAdd", "checkRecallTypes"):
        y = chk(n, y)
    s.bar(h, right_=("butRun",))
