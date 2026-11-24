"""FormMountDefEdit on the grid (UI-12 slice 13, layout script on the planner's tree; the harness builds it with a four-image sample mount,
Scan/SampleData.cs). The window is the mount's drawing (controlDrawing, drawn by the code: CalcRatio fits the mount left of panelRight)
and the settings column panelRight; Description, Width and Height (with their labels) are the window's controls drawn over panelRight.
The planner moved the drawing and the column 8 px in, widened the column to 286 px for a 146 px field column (the combo past its edge),
left Width and Height over the colour rows and Flip on Acquire on the field column. The drawing and the column at main's bounds (the drawing
edge to edge as on main); in the 208 px column: Description and Default Cat as captions over boxes of the content's width, Width, Height
and the three colours as label + field rows on one field column, the three check boxes as a list, Measurement Scale with its own label
column, Generate with its note beside it, Add Item, Item Order (Up and Down with their notes, Reorder All, its note as tall as its lines),
the warning Load shows (hidden in the designer) under it, the save note over the bar, Delete and Save on the bar. The window grows down to
hold the column (main cut the notes)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)

PX = 1022  # panelRight's x in the window (main)
PW = 208  # its width (main)
CW = PW - 16


def caption(h, cap, box, y, form_box=False):
    h.text_align(cap, "BottomLeft")
    h.set(cap, 8, y, CW, 17)
    y += 17 + 2
    h.set(box, (PX + 8) if form_box else 8, y, CW, h.H)
    return y + h.H + 8


def layout(h):
    y = caption(h, "label1", "textDescription", 8, form_box=True)
    y = caption(h, "label13", "comboDefaultCat", y)
    # label + field rows: Width and Height (the window's), the three colours (the column's); one field column
    lw = max(h.lw(n) for n in ("label3", "label4", "labelColor", "label8", "label9"))
    fx = 8 + lw + 6
    for label, field in (("label3", "textWidth"), ("label4", "textHeight")):
        h.text_align(label, "MiddleRight")
        h.set(label, PX + 8, y, lw, h.H)
        h.set(field, PX + fx, y, None, h.H)
        y += 27
    for label, field in (("labelColor", "butColorBack"), ("label8", "butColorFore"), ("label9", "butColorTextBack")):
        h.text_align(label, "MiddleRight")
        h.set(label, 8, y, lw, h.H)
        h.set(field, fx, y, None, h.H)
        y += 27
    y += 8 - 4
    for c in ("checkTransparent", "checkFlipOnAcquire", "checkAdjModeAfterSeries"):
        h.check_left(c)
        h.set(c, 8, y, None, h.CH)
        y += h.CH + 4
    y += 4
    # Measurement Scale: its own label column from the group's content edge
    h.set("groupBoxOD1", 8, y, CW, None)
    glw = max(h.lw(n) for n in ("label11", "label12", "label10"))
    for i, (label, field) in enumerate((("label11", "textUnits"), ("label12", "textScale"), ("label10", "textDecimals"))):
        h.text_align(label, "MiddleRight")
        h.set(label, 8, 23 + i * 27, glw, h.H)
        h.set(field, 8 + glw + 6, 23 + i * 27, CW - 16 - glw - 6, h.H)
    h.set("groupBoxOD1", None, None, None, 23 + 2 * 27 + h.H + 8)
    y += h.n("groupBoxOD1").nh + 8
    # Generate and its note
    g = h.n("butGenerate")
    nx = 8 + g.nw + 6
    nh = h.note_h("label2", CW - (nx - 8))
    h.text_align("label2", "TopLeft")
    h.set("butGenerate", 8, y)
    h.set("label2", nx, y + 4, CW - (nx - 8), nh)
    y += max(g.nh, nh + 4) + 8
    h.set("butAdd", 8, y)
    y += h.n("butAdd").nh + 8
    # Item Order
    h.set("groupBox1", 8, y, CW, None)
    gy = 23
    for but, note in (("butUp", "label6"), ("butDown", "label7")):
        b = h.n(but)
        h.set(but, 8, gy)
        n = h.n(note)
        h.set(note, 8 + b.nw + 6, gy + (b.nh - n.nh) // 2)
        gy += b.nh + 4
    h.set("butReorder", 8, gy)
    gy += h.n("butReorder").nh + 8
    rh = h.note_h("labelReorder", CW - 16)
    h.set("labelReorder", 8, gy, CW - 16, rh)
    h.set("groupBox1", None, None, None, gy + rh + 8)
    y += h.n("groupBox1").nh + 8
    # the warning Load shows when an item is off the mount (hidden in the designer)
    wh = h.note_h("labelWarning", CW)
    h.set("labelWarning", 8, y, CW, wh)
    y += wh + 8
    # the save note over the bar, Delete and Save on the bar
    sh = h.note_h("label5", CW)
    h.text_align("label5", "TopLeft")
    h.set("label5", 8, y, CW, sh)
    y += sh + s.SECTION
    hh = y + 24 + 8
    h.set(".", None, None, PX + PW, hh)
    h.set("panelRight", PX, 0, PW, hh)
    h.set("controlDrawing", 1, 0, PX - 1, hh - 2)
    h.set("butDelete", 8, y, None, 24)
    h.set("butSave", PW - 8 - h.n("butSave").nw, y, None, 24)
