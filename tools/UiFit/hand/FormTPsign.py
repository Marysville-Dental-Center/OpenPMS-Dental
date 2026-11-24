"""FormTPsign on the grid (UI-12 slice 16): the preview between the docked toolbar and the docked signature panel with the grid's
padding on every side (the window's LayoutPreview does the same). The signature panel (122 px, from 115): each box 8 px from the panel's top,
its two-line caption right-aligned 6 px before it, the typed-signature row (hidden until a typed signature is used) 4 px under the box, a
field's height, its label before it; Save bottom right, 8 px from the panel's edges. The Change User button the code adds stands under the
caption, ending 6 px before the box."""
import importlib.util
import os
_spec = importlib.util.spec_from_file_location("_s16", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s16.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


KEEP_BAR_GAP = True  # Save is the signature panel's, beside the boxes


def layout(h):
    W, H = h.tree.w, h.tree.h
    hp = 122
    h.set("panelSig", 0, H - hp, W, hp)
    yType = s.PAD + 79 + 4
    for cap, box, lab, txt in (("labelSig", "signatureBoxWrapper", "labelTypeSig", "textTypeSig"),
                               ("labelSigPractice", "signatureBoxWrapperPractice", "labelTypeSigPractice", "textTypeSigPractice")):
        b = h.n(box)
        h.set(box, b.nx, s.PAD, b.nw, 79)
        h.set(cap, b.nx - s.ITEM - 148, s.PAD, 148, 41)
        h.set(txt, b.nx, yType, b.nw, h.H)
        h.text_align(lab, "MiddleRight")
        h.set(lab, b.nx - s.ITEM - 148, yType, 148, h.H)
    h.set("butSave", W - s.PAD - 75, hp - s.PAD - s.BUTTON_H, 75, s.BUTTON_H)
    tb = h.n("ToolBarMain").nh
    h.set("previewContr", s.PAD, tb + s.PAD, W - 2 * s.PAD, H - hp - tb - 2 * s.PAD)
