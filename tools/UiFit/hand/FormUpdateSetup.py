"""FormUpdateSetup on the grid (UI-12 slice 11, layout script on the planner's tree). The planner kept main's 20 px boxes at a 24 px
pitch, the Web Proxy group at x 69 (on no column), the Recopy note at 12 and its button 39 px under it, and the wide boxes ending 50 px short
of the right padding. Every row at the field height and the row pitch on the label and field columns (the group's labels and boxes too);
the group and the wide boxes from the padding to the right padding; the Recopy note right-aligned in the label column with Recopy on its
first line at the field column."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)
import grid_tools as gt  # noqa: E402

RECOPY_DB = r"Recopy all of the files from C:\Program Files (x86)\Open PMS - Dental\ into a special place in the database for future use in" \
    r" updating other computers."


def layout(h):
    R = s.W(h) - 8
    s.bar(h, right=("butSave",), bh=24)
    x = h.n("textUpdateServerAddress").nx
    wide = R - 8 - x
    y = 8
    for label, field in (("label1", "textUpdateServerAddress"), ("label3", "textWebsitePath")):
        h.set(field, x, y, wide, h.H)
        s.label_before(h, label, field)
        y += h.H + 4
    g = "groupBox1"
    gx = 8
    h.set(g, gx, y + 4, R - gx, None)
    gy = 23
    fx = x - gx
    for label, field, fw in (("label7", "textWebProxyAddress", R - 8 - x), ("label8", "textWebProxyUserName", None),
                             ("label9", "textWebProxyPassword", None)):
        h.set(field, fx, gy, fw, h.H)
        s.label_before(h, label, field)
        gy += h.H + 4
    h.set(g, None, None, None, gy - 4 + 8)
    y = y + 4 + (gy - 4 + 8) + 8
    m = h.n("textMultiple")
    h.set("textMultiple", x, y)
    h.text_align("label5", "TopRight")
    h.set("label5", x - 6 - h.n("label5").nw, y + 3)
    h.set("label6", x + m.nw + 6, y + 3, R - (x + m.nw + 6), None)
    y += m.nh + 4
    c = h.n("checkShowMsi")
    h.set("checkShowMsi", x, y)
    h.set("label10", x + c.nw + 6, y + (c.nh - h.n("label10").nh) // 2)
    y += c.nh + 4
    h.set("textUpdateTime", x, y, None, h.H)
    s.label_before(h, "label12", "textUpdateTime")
    t = h.n("textUpdateTime")
    h.set("butChangeTime", x + t.nw + 6, y, None, h.H)
    y += h.H + 4
    # Fix round: with the A to Z folder not local, Load gives the note a longer text (grid-runtime-texts/11.tsv, 796 px on one line, four
    # lines at the note's width): the note as tall as the wrap of the longest of its texts.
    rw = x - 6 - 8
    rh = max(h.note_h("labelRecopy", rw), gt.measured_lines(RECOPY_DB, max(h.runtime_widths("labelRecopy")), rw) * 16 + 2)
    h.text_align("labelRecopy", "TopRight")
    h.set("labelRecopy", 8, y + 3, rw, rh)
    h.set("butRecopy", x, y, None, 24)
