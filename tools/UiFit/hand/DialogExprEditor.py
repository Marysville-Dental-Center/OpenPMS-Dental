"""The report designer's DialogExprEditor on the grid (UI-12 slice 15, layout script; designer code in DialogExprEditor.cs). Its Layout
handler places the two boxes from the window's outer size (they ran into the right edge and 4 px over the bar); slice 15 lays them out
from the client area with the grid's numbers (DialogExprEditor_Layout), and the designer matches what the handler makes at main's 440 x
310: each caption over its box, the boxes (440 - 16 - 32 - 12) / 2 = 190 px wide with ">>" 6 px between them at their top, down to a
section gap over the bar; OK and Cancel bottom right."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    W0, H0 = 440, 310
    h.set(".", None, None, W0, H0)
    top = s.bar(h, right=("bOK", "bCancel"))
    bw = (W0 - 2 * 8 - 32 - 2 * s.ITEM) // 2
    y = s.caption(h, "lOp", 8, 8, bw)
    bh = top - s.SECTION - y
    h.set("tvOp", 8, y, bw, bh)
    h.set("bCopy", 8 + bw + s.ITEM, y, 32, h.H)
    x = 8 + bw + s.ITEM + 32 + s.ITEM
    s.caption(h, "lExpr", x, 8, W0 - 8 - x)
    h.set("tbExpr", x, y, W0 - 8 - x, bh)
