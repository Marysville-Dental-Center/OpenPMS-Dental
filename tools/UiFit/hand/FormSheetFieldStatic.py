"""FormSheetFieldStatic on the grid (UI-12 slice 11, layout script on the planner's tree). The planner kept main's 18 px left margin for the
text box, a 19 px gutter to the field list and the list 3 px short of the right padding. The text box from the padding to 8 px before the
list; the list ending on the right padding and down to the section gap over the bar; the insert note's right edge on the text box's. The
TextW and TextH readouts after their boxes (fix round)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    yb = s.bar(h, left=("butDelete",), right=("butSave",))
    lst = h.n("listFields")
    h.set("listFields", None, None, R - lst.nx, yb - s.SECTION - lst.ny)
    h.set("textFieldValue", 8, None, lst.nx - 8 - 8, None)
    n = h.n("label1")
    h.set("label1", lst.nx - 8 - n.nw, None)
    # Fix round: TextW and TextH are readouts (UpdateTextSizeLabels sets "TextW: <n>", grid-runtime-texts/11.tsv "TextW: 9999"), not
    # captions: MiddleLeft as on main, 6 px after the Width and Height boxes up to 6 px before the Calculate buttons, on the boxes' rows.
    for label, box, but in (("labelTextW", "textWidth", "butCalcWidth"), ("labelTextH", "textHeight", "butCalcHeight")):
        b = h.n(box)
        x = b.nx + b.nw + 6
        w = h.n(but).nx - 6 - x
        assert w >= h.lw(label), (label, w, h.lw(label))
        h.text_align(label, "MiddleLeft")
        h.set(label, x, b.ny, w, h.H)
