"""FormRepeatChargeEditMulti on the grid (UI-12 slice 10, layout script on the planner's tree). The two-line notes made rows 4 to 6 taller and
their labels stood 4 to 5 px over their fields: one row per field, its label and its note on the field's top (the note's first line on the
field's text), the rows 4 px apart (the pitch where the note is one line), the fields one width, the notes at one x to the right padding.
The window 12 px under the last note for Run (305 -> 266 high; the code does not size it)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    rows = (("textProcCode", "label1", "label5"), ("textChargeAmount", "label2", "label6"), ("textChargeAmountNew", "label11", "label10"),
            ("textPatNumSuperFamilyHead", "label3", "label4"), ("comboIsEnabled", "label12", "label7"), ("textDateStart", "label8", "label9"))
    x = h.n("textProcCode").nx
    y = 8
    for f, l, n in rows:
        h.set(f, x, y, 101, 23)
        s.label_before(h, l, f)
        nw = R - (x + 101 + 6)
        h.set(n, x + 101 + 6, y + 3, nw, max(18, h.note_h(n, nw)))
        y += max(27, 3 + h.n(n).nh + 4)
    n = h.n("label9")
    s.size(h, h.n(".").nw, n.ny + n.nh + 12 + h.n("butRun").nh + 8)
    s.bar(h, right_=("butRun",))
