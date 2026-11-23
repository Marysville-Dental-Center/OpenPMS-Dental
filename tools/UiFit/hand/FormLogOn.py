"""FormLogOn on the grid (UI-12 slice 7, layout script on the planner's tree). The code names the user list "Domain user" with domain login
(grid-runtime-texts), 30 px wider than "User": the labels' column as wide as that text, the filter box, the list and its hidden text box 6
px after it; the password column 16 px after the list (its label 6 px before the box, the box to the right edge, the badge note and the
hidden check boxes on the box's x)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    lw = max(h.lw("label1"), h.lw("labelFilterName"))
    F = 8 + lw + 6
    for n in ("textFilterName", "listUser", "textUser"):
        s.move(h, n, x=F)
    s.label_before(h, "labelFilterName", "textFilterName", w=lw)
    lu = h.n("listUser")
    h.set("label1", F - 6 - lw, lu.ny, lw, h.H)
    h.text_align("label1", "MiddleRight")
    lw2 = h.lw("label2")
    P = F + lu.nw + 16 + lw2 + 6
    s.span(h, "textPassword", x=P, r=s.right(h))
    s.label_before(h, "label2", "textPassword", w=lw2)
    for n in ("labelSwipeBadge", "checkShowCEMTUsers", "checkUseDomainLogin"):
        s.move(h, n, x=P)
