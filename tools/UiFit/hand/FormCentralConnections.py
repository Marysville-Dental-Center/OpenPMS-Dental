"""FormCentralConnections on the grid (UI-12 slice 12, layout script on the planner's tree). Main's 12 px grid margin and the planner's
15 px growth undone (main's 859 x 483): the Connection Groups and Search rows on the first row, Search ending on the grid's right edge; the
grid from the padding under them down to the bottom padding; the column beside it 208 px wide (the Dynamic Mode check box's two lines fit
it): Add, the Conn. Order and Prefs groups at the sibling gap, their texts from their content edge, and OK and Cancel on the bottom right."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 859, 483)
    R = 859 - s.PAD
    B = 483 - s.PAD
    cw = 208
    cx = R - cw
    gr = cx - s.GAP
    h.set("label3", s.PAD, s.PAD, h.lw("label3"), h.H)
    h.text_align("label3", "MiddleRight")
    h.set("comboConnectionGroups", s.PAD + h.lw("label3") + s.LGAP, s.PAD, None, h.H)
    t = h.n("textSearch")
    h.set("textSearch", gr - t.nw, s.PAD, None, h.H)
    s.gap_before(h, "label1", "textSearch")
    top = s.PAD + h.H + s.GAP
    h.set("gridMain", s.PAD, top, gr - s.PAD, B - top)
    h.set("butAdd", cx, top, 75, 24)
    y = top + 24 + s.GAP
    # Conn. Order: Up, Down and Alphabetize at the pitch of buttons, the note after Alphabetize.
    by = s.TITLE
    for name in ("butUp", "butDown", "butAlphabetize"):
        h.set(name, s.PAD, by, 77, 24)
        by += 24 + s.ROWGAP
    a = h.n("butAlphabetize")
    h.set("label2", s.PAD + 77 + s.LGAP, a.ny, h.lw("label2"), 24)
    h.text_align("label2", "MiddleLeft")
    h.set("groupOrdering", cx, y, cw, by - s.ROWGAP + s.PAD)
    y += h.n("groupOrdering").nh + s.GAP
    # Prefs: the two note lines, then the two check boxes as a list.
    ccw = cw - 2 * s.PAD
    py = s.note(h, "label4", s.PAD, s.TITLE, ccw, estimate=False)
    py = s.note(h, "label5", s.PAD, py, ccw, estimate=False) + s.ROWGAP
    h.check_left("checkIsAutoLogon")
    h.set("checkIsAutoLogon", s.PAD, py, h.cw("checkIsAutoLogon"), h.CH)
    py += h.CH + s.ROWGAP
    h.check_left("checkUseDynamicMode")
    h.text_align("checkUseDynamicMode", "MiddleLeft")
    h.set("checkUseDynamicMode", s.PAD, py, ccw, 34)
    h.set("groupPrefs", cx, y, cw, py + 34 + s.PAD)
    s.bar(h, right=("butOK", "butCancel"), bh=24)
