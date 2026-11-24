"""UserControlSetupWizPractice on the grid (UI-12 slice 11, layout script on the planner's tree). The planner moved the Default Provider
list and its caption past the page's right edge (935 on a 930 px page; main has them right of the title rows), left the labels
left-aligned 234 px wide (the fields at 264) and the address group at x 170 (on no column). One label column right-aligned before one field
column for the page and the address group (the group from the padding, its labels on the page's label column); the Default Provider list
on a right column 250 wide ending on the right padding, its caption over it, down to the group's bottom; the wide boxes ending 8 px before
that column; the note and Advanced ending on the right padding under the group."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    lw = max([h.lw(n) for n in ("label3", "label9", "label19")] + [h.lw(n) + 8 for n in ("label5", "label16", "label2", "label1", "label6")])
    fx = 8 + lw + 6
    colx = R - 250
    fr = colx - 8 - 8
    y = 8
    for label, field in (("label3", "textPracticeTitle"), ("label9", "textPhone"), ("label19", "textFax")):
        h.set(field, fx, y, fr - fx, h.H)
        h.text_align(label, "MiddleRight")
        h.set(label, 8, y, lw, h.H)
        y += h.H + 4
    gy = y + 4
    h.set("groupBox2", 8, gy, fr + 8 - 8, None)
    iy = 23
    for label, field, full in (("label5", "textAddress", True), ("label16", "textAddress2", True), ("label2", "textCity", True),
                               ("label1", "textST", False), ("label6", "textZip", False)):
        h.set(field, fx - 8, iy, (fr - 8 - fx) if full else None, h.H)
        h.text_align(label, "MiddleRight")
        h.set(label, 8, iy, lw - 8, h.H)
        iy += h.H + 4
    gh = iy - 4 + 8
    h.set("groupBox2", None, None, None, gh)
    h.text_align("label10", "BottomLeft")
    h.set("label10", colx, 8, 250, 18)
    h.set("listProvider", colx, 8 + 18 + 2, 250, gy + gh - (8 + 18 + 2))
    ny = gy + gh + 8
    n = h.n("label4")
    h.set("label4", R - n.nw, ny)
    a = h.n("butAdvanced")
    h.set("butAdvanced", R - a.nw, ny + n.nh + 4, None, 24)
