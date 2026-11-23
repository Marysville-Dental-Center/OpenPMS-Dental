"""FormStateAbbrEdit on the grid (UI-12 slice 9, layout script on the planner's tree): Medicaid ID Length wrapped to two lines
(GridRulesTests: its row 5 px further down than the pitch): the label on one line, the label column as wide, the three rows on the pitch;
the Description box ends on the right content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W = h.n(".").nw
    lw = max(h.lw(n) for n in ("label1", "label2", "labelMedIDLength"))
    F = 8 + lw + 6
    y = 8
    for lab, box in (("label1", "textDescription"), ("label2", "textAbbr"), ("labelMedIDLength", "textMedIDLength")):
        h.set(lab, 8, y, lw, h.H)
        h.text_align(lab, "MiddleRight")
        s.move(h, box, x=F, y=y)
        y += h.H + 4
    s.span(h, "textDescription", r=W - 8)
