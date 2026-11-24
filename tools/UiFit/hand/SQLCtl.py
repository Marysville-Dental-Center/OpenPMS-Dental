"""The report designer's SQLCtl on the grid (UI-12 slice 15, layout script; designer code in SQLCtl.cs). Main docked the tables tree
to the window's left edge (it touched the window's edges) and the planner widened the window to 494. The tree on the padding (no longer
docked: anchored to the top, bottom and left, as wide as main's 136), ">>" 6 px after it, the SQL box after that to the right padding,
both down to the bar panel; the bar panel 43 px (OK and Cancel 12 px under the boxes, on the bottom and right padding); main's 480 x 182."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



KEEP_BAR_GAP = True


def layout(h):
    W0, H0 = 480, 182
    h.set(".", None, None, W0, H0)
    ph = s.SECTION + 23 + 8
    h.set("panel1", 0, H0 - ph, W0, ph)
    h.set("bCancel", W0 - 8 - 75, s.SECTION)
    h.set("bOK", W0 - 8 - 75 - 8 - 75, s.SECTION)
    bh = H0 - ph - 8
    h.set("tvTablesColumns", 8, 8, 136, bh)
    h.set("bMove", 8 + 136 + s.ITEM, 8, 32, h.H)
    x = 8 + 136 + s.ITEM + 32 + s.ITEM
    h.set("tbSQL", x, 8, W0 - 8 - x, bh)
