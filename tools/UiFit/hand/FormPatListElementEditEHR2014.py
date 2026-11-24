"""FormPatListElementEditEHR2014 on the grid (UI-12 slice 11, layout script on the planner's tree; slice 8's FormEhrPatListElementEdit
is its sibling). The Restriction and Operand lists were 75 px wide, cut their items (Medicatio, GreaterTha) and their last rows. Both as
wide as the field column's boxes (136), Restriction four whole rows of UI.ListBox's 17 px line, Operand all three; every row under them
at the row pitch, each caption on its row (the Lab value rows the code shows for a lab result keep their place in the order)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


LINE = 17
BORDER = 4


def layout(h):
    w = h.n("textSNOMED").nw
    y = 8
    rows = (("labelRestriction", "listRestriction", BORDER + 4 * LINE, ()), ("labelOperand", "listOperand", BORDER + 3 * LINE, ()),
            ("labelCompareString", "textCompareString", h.H, ("butPicker", "labelExample")),
            ("labelSNOMED", "textSNOMED", h.H, ("butSNOMED", "butProblem")),
            ("labelLabValue", "textLabValue", h.H, ("comboUnits",)),
            ("labelLabValueType", "comboLabValueType", h.H, ("labelProblemSuggest",)),
            ("labelAfterDate", "textDateStart", h.H, ()), ("labelBeforeDate", "textDateStop", h.H, ()))
    for label, field, hh, after in rows:
        f = h.n(field)
        h.set(field, None, y, w if field.startswith("list") else None, hh)
        h.set(label, None, y)
        for a in after:
            n = h.n(a)
            dy = (h.H - n.nh) // 2 if n.nh < h.H else 0
            h.set(a, None, y + dy)
        y += hh + 4
    # Fix round: the code captions the compare string up to "Communication Preference" (grid-runtime-texts/11.tsv, 153 px; the designer's
    # "Compare string" 89): the label right-aligned before its box as wide as that text (it fits the label column: the field column stays).
    s.label_before(h, "labelCompareString", "textCompareString")
