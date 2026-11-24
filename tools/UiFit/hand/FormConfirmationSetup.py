"""FormConfirmationSetup on the grid (UI-12 slice 11, layout script on the planner's tree; its designer file is FormConfirmationsSetup).
The planner kept main's 12 px grid margin, the two status rows 21 px tall with their captions 2 px from the boxes and 4 px low, Save 27 px
tall, and Group Families floating 26 px under the grid. Save on the bar; the two status rows above it at the row pitch, the second on the
bar's line, captions right-aligned 6 px before the boxes; Group Families on the first status row at the right padding; the grid from the
padding to the right padding, down to the section gap over the first row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    yb = s.bar(h, right=("butSave",), bh=24)
    y2 = yb
    y1 = y2 - h.H - 4
    lw = max(h.lw("label5"), h.lw("label6"))
    for label, field, y in (("label5", "comboStatusEmailedConfirm", y1), ("label6", "comboStatusTextMessagedConfirm", y2)):
        h.text_align(label, "MiddleRight")
        h.set(label, 8, y, lw, h.H)
        h.set(field, 8 + lw + 6, y, None, h.H)
    c = h.n("checkGroupFamilies")
    h.set("checkGroupFamilies", R - c.nw, y1 + (h.H - c.nh) // 2)
    h.set("gridMain", 8, 8, R - 8, y1 - s.SECTION - 8)
