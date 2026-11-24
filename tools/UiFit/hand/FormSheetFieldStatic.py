"""FormSheetFieldStatic on the grid (UI-12 slice 11, layout script on the planner's tree). The planner kept main's 18 px left margin for the
text box, a 19 px gutter to the field list and the list 3 px short of the right padding. The text box from the padding to 8 px before the
list; the list ending on the right padding; the insert note's right edge on the text box's."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    s.bar(h, left=("butDelete",), right=("butSave",))
    lst = h.n("listFields")
    h.set("listFields", None, None, R - lst.nx, None)
    h.set("textFieldValue", 8, None, lst.nx - 8 - 8, None)
    n = h.n("label1")
    h.set("label1", lst.nx - 8 - n.nw, None)
