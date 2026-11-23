"""FormBlockoutCutCopyPaste on the grid (UI-12 slice 6 fix round, layout script on the planner's tree). The planner kept main's column at
x 26 in a 290 px window (92 px empty on the right; main drew it so), the Clipboard Contents box 21 px short of the Copy group's right
border and Repeat 23 px tall beside the 24 px Paste: the column at the padding with Include Weekends 6 px after Clear Day; in the Copy
group the note as tall as its lines at the group's width, Clipboard Contents and its box to the content edge, Copy Day and Copy Week under
it 4 px apart; in the Paste group Repeat 75 x 24 with its # box after it.
Slice 8 (the lead's decision on slice 6's question: a window never shrinks): the window keeps main's 290 x 383 and both groups span the
content width at the 8 px margins (274 wide); the # caption 8 px after Repeat and its box 6 px after the caption (one row, as main draws
it); the height the content does not fill is shared out as equal gaps between the Clear Day row, the two groups and the bottom edge (no
empty band at the bottom)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)

MAIN_W, MAIN_H = 290, 383  # main's client size (the designer before slice 6)


def layout(h):
    cb = h.n("butClearDay")
    h.set("butClearDay", 8, 8)
    ck = h.n("checkWeekend")
    h.set("checkWeekend", 8 + cb.nw + 6, None)
    gw = MAIN_W - 16
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
    g1h = y + 24 + 8
    # The Paste group: Replace Existing, Paste, Repeat with the # caption and box after it.
    yr = h.n("butRepeat").ny
    h.set("butRepeat", 8, yr, 75, 24)
    l4 = h.n("label4")
    xl = 8 + 75 + 8
    h.set("label4", xl, yr)
    h.set("textRepeat", xl + l4.nw + 6, yr)
    g2h = yr + 24 + 8
    # The spare height: equal gaps under the Clear Day row, between the groups and under the Paste group.
    top = 8 + 24
    spare = MAIN_H - top - g1h - g2h
    gap = spare // 3
    y1 = top + gap
    y2 = y1 + g1h + gap
    h.set("groupBox1", 8, y1, gw, g1h)
    h.set("groupBox2", 8, y2, gw, g2h)
    h.set(".", None, None, MAIN_W, MAIN_H)
    e.icon_buttons(h)
