"""FormDoseSpotPropertyEdit on the grid (UI-12 slice 11, layout script on the planner's tree; its designer file is
FormDoseSpotProperyEdit). The planner kept main's block in the middle: the labels from x 31 and the buttons ending 24 px short of the right
padding. The label column from the padding, the boxes after it, Register Clinic and Clear ID/Key ending on the right padding (Save's edge),
the boxes up to 6 px before them; Show key on the field column."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    s.bar(h, right=("butSave",))
    lw = max(h.lw(n) for n in ("label3", "label1", "label2"))
    fx = 8 + lw + 6
    bx = R - h.n("butRegisterClinic").nw
    for label, field, but in (("label3", "textClinicAbbr", "butRegisterClinic"), ("label1", "textClinicID", "butClear"),
                              ("label2", "textClinicKey", None)):
        f = h.n(field)
        h.set(field, fx, None, bx - 6 - fx, h.H)
        h.text_align(label, "MiddleRight")
        h.set(label, 8, f.ny, lw, h.H)
        if but:
            h.set(but, bx, f.ny, None, h.H)
    h.set("checkShowKey", fx, None)
