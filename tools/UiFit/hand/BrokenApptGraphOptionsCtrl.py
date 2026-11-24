"""BrokenApptGraphOptionsCtrl on the grid (UI-12 slice 12, layout script on the planner's tree). The descriptions beside the three radio
buttons started at three x (102, 119, 110) and the two type combo boxes at a fourth: the radio buttons as a column from the group's content
edge, each description on its radio button's row and each combo box under its description, all on one column after the widest radio
button; the group as wide as the widest description and the control as large as the group and the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    radios = ("radioRunProcs", "radioRunApts", "radioRunAdjs")
    for r in radios:
        h.check_left(r)
    cx = s.PAD + max(h.cw(r) for r in radios) + s.LGAP
    y = s.TITLE
    right = cx
    for r, lab, combo in (("radioRunProcs", "label1", "comboBrokenProcType"), ("radioRunApts", "label2", None),
                          ("radioRunAdjs", "label3", "comboAdjType")):
        h.set(r, s.PAD, y, h.cw(r), h.CH)
        h.set(lab, cx, y, h.lw(lab), h.CH)
        h.text_align(lab, "MiddleLeft")
        right = max(right, cx + h.lw(lab))
        y += h.CH + s.ROWGAP
        if combo:
            h.set(combo, cx, y, None, h.H)
            y += h.H + s.ROWGAP
    gw = right + s.PAD
    gh = y - s.ROWGAP + s.PAD
    h.set("groupBox3", s.PAD, s.PAD, gw, gh)
    s.size(h, gw + 2 * s.PAD, gh + 2 * s.PAD)
