"""FormClaimFormItemEdit on the grid (UI-12 slice 7, layout script on the planner's tree). The Format String note was drawn over the line
under the file name note (main has the same overlap), the left column stood 12 px from the left and the field list ended 85 px short of
Save's edge: the left column on the content edge, each note as tall as its lines (three and four, as GridRulesTests measures them) with the section gap before Format String, the field
list to Save's edge and down to 8 px over the bottom bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W = 240
    for n in ("labelImageFileName", "textImageFileName", "label2", "labelFormatString", "textFormatString"):
        h.set(n, 8, None, W, None)
    y = h.n("textImageFileName").ny + h.H + 4
    nh = 46
    h.set("label2", None, y, None, nh)
    y += nh + 12
    nh = 60
    h.set("labelFormatString", None, y, None, nh)
    y += nh + 2
    h.set("textFormatString", None, y, None, None)
    s.span(h, "listFieldName", x=8 + W + 6, r=s.right(h), b=h.n("butSave").ny - 8)
    s.move(h, "labelFieldName", x=8 + W + 6)
