"""FormBlockoutCutCopyPaste on the grid (UI-12 slice 6 fix round, layout script on the planner's tree). The planner kept main's column at
x 26 in a 290 px window (92 px empty on the right; main drew it so), the Clipboard Contents box 21 px short of the Copy group's right
border and Repeat 23 px tall beside the 24 px Paste: the column at the padding with Include Weekends 6 px after Clear Day; both groups
from the padding to the check box's right edge (one right edge for the window's content), the window 8 px past it (main's 290 left 79 px
empty; the code does not size the window); in the Copy group the note as tall as its lines at the group's width, Clipboard Contents and its
box to the content edge, Copy Day and Copy Week under it 4 px apart; in the Paste group Repeat 75 x 24 and the # box on the content edge
with its caption 6 px before it; the window's height down to 8 px under the Paste group."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    cb = h.n("butClearDay")
    h.set("butClearDay", 8, 8)
    ck = h.n("checkWeekend")
    xk = 8 + cb.nw + 6
    h.set("checkWeekend", xk, None)
    R = xk + ck.nw
    gw = R - 8
    cw = gw - 16
    # The Copy group: note, caption, box, Copy Day, Copy Week.
    nh = h.note_h("label1", cw)
    y = 23
    h.set("label1", 8, y, cw, nh)
    y += nh + 2
    h.set("label3", 8, y, cw, None)
    y += h.n("label3").nh + 2
    h.set("textClipboard", 8, y, cw, None)
    y += h.n("textClipboard").nh + 4
    h.set("butCopyDay", 8, y)
    y += 24 + 4
    h.set("butCopyWeek", 8, y)
    y += 24 + 8
    g1 = h.n("groupBox1")
    h.set("groupBox1", 8, g1.ny, gw, y)
    # The Paste group: Replace Existing, Paste, Repeat with the # box on the content edge.
    yr = h.n("butRepeat").ny
    h.set("butRepeat", 8, yr, 75, 24)
    tr = h.n("textRepeat")
    xr = gw - 8 - tr.nw
    h.set("textRepeat", xr, yr)
    l4 = h.n("label4")
    h.set("label4", xr - 6 - l4.nw, yr)
    y2 = g1.ny + y + 8
    g2h = yr + 24 + 8
    h.set("groupBox2", 8, y2, gw, g2h)
    h.set(".", None, None, R + 8, y2 + g2h + 8)
    e.icon_buttons(h)
