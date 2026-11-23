"""FormProgramLinkEdit on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree). The planner paired the "Text on
button" label with the custom bridge note beside it and left its box a row lower beside the Plug-in dll name label, the plug-in box a row
lower still; the Notes caption stood at the clinic picker, not over its box. As main has them: the note under the command line row, then
Text on button and Plug-in dll name (with its example 6 px after the box) on the 27 px pitch, the grid under them; the Notes caption 2 px
over its box; the Button Settings group and the clinic note at the content edge (8)."""


def layout(h):
    H = h.H
    P = H + 4
    F = h.n("textButtonText").nx
    note = h.n("label9")
    y = note.ny + note.nh + 4
    for lab, fld in (("label7", "textButtonText"), ("label5", "textPluginDllName")):
        lw = h.lw(lab)
        h.set(lab, F - 6 - lw, y, lw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F, y, None, H)
        y += P
    f = h.n("textPluginDllName")
    h.set("label11", f.nx + f.nw + 6, f.ny, h.lw("label11"), H)
    h.text_align("label11", "MiddleLeft")
    g = h.n("gridMain")
    h.set("gridMain", None, y, None, g.ny + g.nh - y)
    t = h.n("textNote")
    h.set("label8", t.nx, t.ny - 2 - 17, h.lw("label8"), 17)
    h.text_align("label8", "BottomLeft")
    h.set("groupboxPLbuttons", 8, None)
    h.set("labelDisableForClinic", 8, None)
