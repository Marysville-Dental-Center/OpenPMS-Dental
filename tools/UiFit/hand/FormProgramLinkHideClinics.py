"""FormProgramLinkHideClinics on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the window 451 x 506 -> 459 x
522 and kept the content 12 px from the left, the lists 34 px over Save: main's size, the content from the padding, Visible on the right
padding, the arrows centred between the lists, the lists 12 px over Save."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 451, 506)
    R = s.right(h)
    y = s.bar(h, right_=("butSave",))
    h.set("label3", 8, None, R - 8, None)
    h.set("labelClinicStateWarning", 8, None, None, None)
    s.move(h, "checkOrderAlphabetical", x=R - h.n("checkOrderAlphabetical").nw)
    w = h.n("listboxHiddenClinics").nw
    x2 = R - w
    h.set("labelHidden", 8, None, None, None)
    h.set("labelVisible", x2, None, None, None)
    s.fill(h, "listboxHiddenClinics", r=8 + w, over="butSave")
    s.fill(h, "listboxVisibleClinics", x=x2, over="butSave")
    bx = (8 + w + x2) // 2 - h.n("butRight").nw // 2
    h.set("butRight", bx, None, None, None)
    h.set("butLeft", bx, None, None, None)
