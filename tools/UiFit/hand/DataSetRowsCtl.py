"""The report designer's DataSetRowsCtl on the grid (UI-12 slice 15, layout script on main's layout; designer code in DataSetRowsCtl.cs;
the harness builds it since slice 15, SampleData.BeforePlainHost). Main spaced its button column 9 to 20 px apart and showed Load From SQL
on three lines. The XML file check box with its box and "..." on the first row (to the page's content edge, 440); the rows grid under it
with its buttons beside it on the content edge, 4 px apart (Delete, Up, Down, Clear, Load From SQL on one line, 100 px), the grid down to
the warning, which wraps over the width on the page's bottom padding; main's 488 x 304 (a property page)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    R = s.PAGE_R
    h.set(".", None, None, 488, 304)
    x = s.chk(h, "chkRowsFile", 8, 8) + s.ITEM
    h.set("bRowsFile", R - 24, 8, 24, h.H)
    h.set("tbRowsFile", x, 8, R - 24 - s.ITEM - x, h.H)
    bw = 100
    for i, b in enumerate(("bDelete", "bUp", "bDown", "bClear", "bLoad")):
        h.set(b, R - bw, 35 + 27 * i, bw, h.H)
    nh = h.note_h("label1", R - 8)
    ny = s.PAGE_B - nh
    s.note(h, "label1", 8, ny, R - 8)
    h.set("dgRows", 8, 35, R - bw - 8 - 8, ny - 8 - 35)
