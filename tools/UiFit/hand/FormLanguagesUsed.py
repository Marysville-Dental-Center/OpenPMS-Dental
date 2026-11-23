"""FormLanguagesUsed on the grid (UI-12 slice 8, layout script on the planner's tree). The left column stood at x 29 to 32 (note,
caption, list, Custom Language group), the right column 8 px short of the right padding, the captions 4 px over their lists, and Delete,
Up and Down in a row 72 px short of the list's right edge: the note, the All Languages column and the group on the padding; the
Languages used column on the right padding; Add centred between the two lists; Delete under the right list on its left edge, Up and Down
on its right edge; the captions 2 px over their lists. The custom Add button keeps the 24 px of an icon button (at 23 its arrow
paints over the right border at 150%).
Fix round (ui/ui12-8-2): the Custom Language group was the stock GroupBox (square frame, title cut into the line; now OpenPmsDental.UI.
GroupBox in the designer) and spanned 8..402 with its content ending at 364 and its bottom 13 px over Save's row: the group under the All
Languages list at the list's width (8..296; the list 10 px wider so the note keeps its three lines), the text box and Add on its 8 px content edges under the title band, the note under them
as tall as its lines, the group's bottom on Save's bottom and the list down to 8 px over the group."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


LIST_W = 288


def layout(h):
    R = s.W(h) - 8
    h.set("label2", 8, None)
    la = h.n("listAvailable")
    # The All Languages column 288 wide (was 278): the group under it holds the Custom Language note at 272 px, where its three lines (hard
    # line breaks) stay three (at 262 GridRulesTests' measure wraps two of them).
    h.set("listAvailable", None, None, LIST_W, None)
    lu = h.n("listUsed")
    top = la.ny
    for cap, lst, x in (("label1", "listAvailable", 8), ("label3", "listUsed", R - lu.nw)):
        c = h.n(cap)
        h.set(cap, x, top - 2 - c.nh)
        h.set(lst, x, top)
    xr = R - lu.nw
    ad = h.n("butAdd")
    mid = (8 + la.nw + xr) // 2
    h.set("butAdd", mid - ad.nw // 2, top)
    yb = top + lu.nh + 4
    h.set("butDelete", xr, yb)
    dn = h.n("butDown")
    up = h.n("butUp")
    h.set("butDown", R - dn.nw, yb)
    h.set("butUp", R - dn.nw - 8 - up.nw, yb)
    l5 = h.n("label5")
    y5 = yb + h.n("butDelete").nh + 4
    h.set("label5", xr, y5, lu.nw, None)
    h.set("comboLanguagesIndicateNone", xr, y5 + l5.nh + 2, lu.nw, None)
    s.e.icon_buttons(h)
    gw = la.nw
    cr = gw - 8
    ac = h.n("butAddCustom")
    y = 19 + 4  # the title band (UI.GroupBox, 19 px) and the title gap
    h.set("butAddCustom", cr - ac.nw, y)
    h.set("textCustom", 8, y, cr - ac.nw - 6 - 8, None)
    y += max(ac.nh, h.n("textCustom").nh) + 4
    nh = 3 * 16 + 2  # the note's three lines (note_h reads the text as one paragraph)
    h.set("label6", 8, y, cr - 8, nh)
    gh = y + nh + 8
    sv = h.n("butSave")
    gy = sv.ny + sv.nh - gh
    h.set("groupBox1", 8, gy, gw, gh)
    h.set("listAvailable", None, None, None, gy - 8 - top)
