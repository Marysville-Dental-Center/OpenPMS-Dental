"""FormPatFieldDefEdit on the grid (UI-12 slice 7, layout script on the planner's tree). The left column stood 9 px from the left against
Delete's 8 and 19 px before the button column, Add 2 px taller than Up, Down and Merge, the column 4 px left of Save's: the left column on
the content edge to 8 px before the buttons, the four buttons 24 px tall, 4 px apart on Save's column."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    bx = h.n("butSave").nx
    for n in ("labelStatus", "labelFieldType", "comboFieldType"):
        s.move(h, n, x=8)
    for n in ("textName", "gridPickListItems"):
        s.span(h, n, x=8, r=bx - 8)
    h.set("labelFieldType", None, None, bx - 16, None)
    h.set("butAdd", None, None, None, 24)
    s.column(h, ("butAdd", "butUp", "butDown", "butMerge"), bx, h.n("butAdd").ny)
